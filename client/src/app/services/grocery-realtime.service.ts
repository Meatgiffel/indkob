import { Injectable, OnDestroy } from '@angular/core';
import { BehaviorSubject, Subject } from 'rxjs';
import { HubConnection, HubConnectionBuilder, HubConnectionState, HttpTransportType, LogLevel } from '@microsoft/signalr';
import { AuthService } from './auth.service';

export type GroceryRealtimeEventType = 'created' | 'updated' | 'deleted' | 'cleared' | 'resync';

export interface GroceryChangedEvent {
  type: GroceryRealtimeEventType;
  entryId: number | null;
  atUtc: string;
}

@Injectable({ providedIn: 'root' })
export class GroceryRealtimeService implements OnDestroy {
  private connection: HubConnection | null = null;
  private startLoopPromise: Promise<void> | null = null;
  private manualStop = false;
  private destroyed = false;

  private readonly changesSubject = new Subject<GroceryChangedEvent>();
  readonly changes$ = this.changesSubject.asObservable();

  private readonly stateSubject = new BehaviorSubject<'disconnected' | 'connecting' | 'connected' | 'reconnecting'>('disconnected');
  readonly state$ = this.stateSubject.asObservable();

  constructor(private auth: AuthService) {
    this.auth.user$.subscribe(user => {
      if (user) {
        void this.ensureStarted();
        return;
      }
      void this.stop();
    });
  }

  async ensureStarted(): Promise<void> {
    if (!this.auth.isAuthenticated) return;
    if (this.destroyed) return;

    this.ensureConnection();

    if (!this.connection) return;
    if (this.connection.state === HubConnectionState.Connected) return;
    if (this.startLoopPromise) return this.startLoopPromise;

    this.manualStop = false;
    this.startLoopPromise = this.startLoop();
    await this.startLoopPromise;
  }

  async stop(): Promise<void> {
    this.manualStop = true;
    const activeStart = this.startLoopPromise;
    if (activeStart) {
      try {
        await activeStart;
      } catch {
        // No-op: stop will still execute.
      }
    }

    if (!this.connection) {
      this.stateSubject.next('disconnected');
      return;
    }

    if (this.connection.state !== HubConnectionState.Disconnected) {
      try {
        await this.connection.stop();
      } catch {
        // No-op: connection is effectively stopped.
      }
    }

    this.stateSubject.next('disconnected');
  }

  ngOnDestroy(): void {
    this.destroyed = true;
    void this.stop();
    this.changesSubject.complete();
    this.stateSubject.complete();
  }

  private async startLoop(): Promise<void> {
    const retryDelaysMs = [0, 1500, 3000, 5000, 10000, 15000];
    let attempt = 0;

    try {
      while (!this.destroyed && !this.manualStop && this.auth.isAuthenticated) {
        this.stateSubject.next(attempt === 0 ? 'connecting' : 'reconnecting');
        try {
          await this.connection!.start();
          this.stateSubject.next('connected');
          this.emitResync();
          return;
        } catch {
          const delay = retryDelaysMs[Math.min(attempt, retryDelaysMs.length - 1)];
          attempt += 1;
          await sleep(delay);
        }
      }
    } finally {
      this.startLoopPromise = null;
      if (this.manualStop || !this.auth.isAuthenticated || this.destroyed) {
        this.stateSubject.next('disconnected');
      }
    }
  }

  private ensureConnection(): void {
    if (this.connection) return;

    const connection = new HubConnectionBuilder()
      .withUrl('/api/hubs/grocery', {
        withCredentials: true,
        transport: HttpTransportType.WebSockets | HttpTransportType.ServerSentEvents | HttpTransportType.LongPolling
      })
      .withAutomaticReconnect([0, 1500, 3000, 5000, 10000])
      .configureLogging(LogLevel.Warning)
      .build();

    connection.on('groceryChanged', (event: GroceryChangedEvent) => {
      this.changesSubject.next({
        type: event?.type ?? 'updated',
        entryId: event?.entryId ?? null,
        atUtc: event?.atUtc ?? new Date().toISOString()
      });
    });

    connection.onreconnecting(() => {
      this.stateSubject.next('reconnecting');
    });

    connection.onreconnected(() => {
      this.stateSubject.next('connected');
      this.emitResync();
    });

    connection.onclose(() => {
      this.stateSubject.next('disconnected');
      if (!this.manualStop && !this.destroyed && this.auth.isAuthenticated) {
        void this.ensureStarted();
      }
    });

    this.connection = connection;
  }

  private emitResync(): void {
    this.changesSubject.next({
      type: 'resync',
      entryId: null,
      atUtc: new Date().toISOString()
    });
  }
}

function sleep(ms: number): Promise<void> {
  return new Promise(resolve => setTimeout(resolve, ms));
}
