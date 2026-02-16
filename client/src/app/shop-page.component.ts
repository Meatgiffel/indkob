import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Subject, auditTime, firstValueFrom, interval, takeUntil } from 'rxjs';
import { CardModule } from 'primeng/card';
import { DropdownModule } from 'primeng/dropdown';
import { ButtonModule } from 'primeng/button';
import { MessageService } from 'primeng/api';

import { ApiService } from './services/api.service';
import { GroceryEntry } from './models';
import { parseHttpError } from './services/http-error';
import { GroceryRealtimeService } from './services/grocery-realtime.service';

type RowFeedbackState = 'idle' | 'saving' | 'saved' | 'error';

type RowFeedback = {
  state: RowFeedbackState;
  showSavingIcon: boolean;
  savingIconDelayHandle: ReturnType<typeof setTimeout> | null;
  resetHandle: ReturnType<typeof setTimeout> | null;
};

@Component({
  selector: 'app-shop-page',
  standalone: true,
  imports: [CommonModule, FormsModule, CardModule, DropdownModule, ButtonModule],
  templateUrl: './shop-page.component.html',
  styleUrl: './shop-page.component.scss'
})
export class ShopPageComponent implements OnInit {
  entries: GroceryEntry[] = [];
  loadingEntries = false;
  doneFilter: 'all' | 'done' | 'open' = 'open';
  readonly doneFilterOptions = [
    { label: 'Alle', value: 'all' as const },
    { label: 'Færdige', value: 'done' as const },
    { label: 'Mangler', value: 'open' as const }
  ];
  private readonly pendingDoneById = new Map<number, boolean>();
  private readonly confirmedDoneById = new Map<number, boolean>();
  private readonly inFlightById = new Set<number>();
  private readonly feedbackById = new Map<number, RowFeedback>();
  confirmingEntryId: number | null = null;
  private confirmResetHandle: ReturnType<typeof setTimeout> | null = null;
  private readonly destroy$ = new Subject<void>();
  private entriesLoadPromise: Promise<void> | null = null;
  private queuedEntriesReload = false;

  constructor(
    private api: ApiService,
    private toast: MessageService,
    private groceryRealtime: GroceryRealtimeService
  ) {}

  ngOnInit(): void {
    void this.loadEntries();
    void this.groceryRealtime.ensureStarted();

    this.groceryRealtime.changes$
      .pipe(
        auditTime(200),
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        void this.loadEntries();
      });

    interval(45000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        void this.loadEntries();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get openCount(): number {
    return this.entries.filter(e => !e.isDone).length;
  }

  get doneCount(): number {
    return this.entries.filter(e => e.isDone).length;
  }

  private get visibleEntries(): GroceryEntry[] {
    if (this.doneFilter === 'done') return this.entries.filter(e => e.isDone || this.isUpdating(e));
    if (this.doneFilter === 'open') return this.entries.filter(e => !e.isDone || this.isUpdating(e));
    return this.entries;
  }

  get groupedEntries(): { area: string; entries: GroceryEntry[] }[] {
    const groups = new Map<string, GroceryEntry[]>();
    for (const entry of this.visibleEntries) {
      const key = entry.itemId ? entry.itemArea || 'Andet' : 'Noter';
      if (!groups.has(key)) {
        groups.set(key, []);
      }
      groups.get(key)!.push(entry);
    }

    return Array.from(groups.entries())
      .sort((a, b) => a[0].localeCompare(b[0], 'da', { sensitivity: 'base' }))
      .map(([area, entries]) => ({
        area,
        entries: [...entries].sort(
          (a, b) =>
            Number(a.isDone) - Number(b.isDone) ||
            (a.itemId ? (a.itemName ?? '') : (a.note ?? '')).localeCompare(b.itemId ? (b.itemName ?? '') : (b.note ?? ''), 'da', { sensitivity: 'base' })
        )
      }));
  }

  async loadEntries(): Promise<void> {
    if (this.entriesLoadPromise) {
      this.queuedEntriesReload = true;
      return this.entriesLoadPromise;
    }

    this.loadingEntries = true;
    this.entriesLoadPromise = (async () => {
      try {
        const entries = await firstValueFrom(this.api.getEntries());
        this.entries = entries;
        this.resetLocalState(entries);
      } catch (err: any) {
        this.toast.add({ severity: 'error', summary: 'Fejl', detail: parseHttpError(err, 'Kunne ikke hente indkøbsseddel.') });
      } finally {
        this.loadingEntries = false;
      }
    })();

    try {
      await this.entriesLoadPromise;
    } finally {
      this.entriesLoadPromise = null;
      if (this.queuedEntriesReload) {
        this.queuedEntriesReload = false;
        void this.loadEntries();
      }
    }
  }

  isUpdating(entry: GroceryEntry): boolean {
    return this.inFlightById.has(entry.id);
  }

  rowFeedbackState(entryId: number): RowFeedbackState {
    const feedback = this.getFeedback(entryId);
    if (feedback.state !== 'saving') return feedback.state;
    return feedback.showSavingIcon ? 'saving' : 'idle';
  }

  async toggleDone(entry: GroceryEntry): Promise<void> {
    if (this.isUpdating(entry)) return;

    if (entry.isDone) {
      this.clearConfirm();
      entry.isDone = false;
      this.pendingDoneById.set(entry.id, false);
      await this.flushEntryUpdate(entry);
      return;
    }

    if (this.confirmingEntryId === entry.id) {
      await this.confirmDone(entry);
      return;
    }

    this.startConfirm(entry.id);
  }

  async confirmDone(entry: GroceryEntry): Promise<void> {
    if (this.isUpdating(entry) || entry.isDone) return;
    this.clearConfirm();
    entry.isDone = true;
    this.pendingDoneById.set(entry.id, true);
    await this.flushEntryUpdate(entry);
  }

  cancelConfirm(entryId?: number): void {
    if (entryId !== undefined && this.confirmingEntryId !== entryId) return;
    this.clearConfirm();
  }

  private resetLocalState(entries: GroceryEntry[]): void {
    for (const feedback of this.feedbackById.values()) {
      this.clearFeedbackTimers(feedback);
    }
    this.pendingDoneById.clear();
    this.confirmedDoneById.clear();
    this.inFlightById.clear();
    this.feedbackById.clear();
    this.clearConfirm();

    for (const entry of entries) {
      this.confirmedDoneById.set(entry.id, entry.isDone);
    }
  }

  private async flushEntryUpdate(entry: GroceryEntry): Promise<void> {
    const entryId = entry.id;
    if (this.inFlightById.has(entryId)) return;

    const desired = this.pendingDoneById.get(entryId);
    if (desired === undefined) return;

    this.pendingDoneById.delete(entryId);
    this.inFlightById.add(entryId);

    const feedback = this.getFeedback(entryId);
    feedback.state = 'saving';
    feedback.showSavingIcon = false;
    this.clearFeedbackTimers(feedback);
    feedback.savingIconDelayHandle = setTimeout(() => {
      const latest = this.feedbackById.get(entryId);
      if (!latest) return;
      if (latest.state === 'saving' && this.inFlightById.has(entryId)) latest.showSavingIcon = true;
    }, 250);

    try {
      await firstValueFrom(
        this.api.updateEntry(entryId, {
          itemId: entry.itemId,
          amount: entry.amount ?? null,
          note: entry.note ?? null,
          isDone: desired
        })
      );

      this.confirmedDoneById.set(entryId, desired);
      feedback.state = 'saved';
      feedback.showSavingIcon = false;
      this.clearFeedbackTimers(feedback);
      feedback.resetHandle = setTimeout(() => {
        const latest = this.feedbackById.get(entryId);
        if (latest?.state === 'saved') latest.state = 'idle';
      }, 900);
    } catch (err: any) {
      const confirmed = this.confirmedDoneById.get(entryId);
      entry.isDone = confirmed ?? !desired;
      this.pendingDoneById.delete(entryId);
      feedback.state = 'error';
      feedback.showSavingIcon = false;
      this.clearFeedbackTimers(feedback);
      feedback.resetHandle = setTimeout(() => {
        const latest = this.feedbackById.get(entryId);
        if (latest?.state === 'error') latest.state = 'idle';
      }, 2000);

      this.toast.add({ severity: 'error', summary: 'Fejl', detail: parseHttpError(err, 'Kunne ikke opdatere status.') });
    } finally {
      this.inFlightById.delete(entryId);
    }

    if (this.pendingDoneById.has(entryId)) {
      await this.flushEntryUpdate(entry);
    }
  }

  private getFeedback(entryId: number): RowFeedback {
    const existing = this.feedbackById.get(entryId);
    if (existing) return existing;
    const created: RowFeedback = { state: 'idle', showSavingIcon: false, savingIconDelayHandle: null, resetHandle: null };
    this.feedbackById.set(entryId, created);
    return created;
  }

  private clearFeedbackTimers(feedback: RowFeedback): void {
    if (feedback.savingIconDelayHandle) {
      clearTimeout(feedback.savingIconDelayHandle);
      feedback.savingIconDelayHandle = null;
    }
    if (feedback.resetHandle) {
      clearTimeout(feedback.resetHandle);
      feedback.resetHandle = null;
    }
  }

  private startConfirm(entryId: number): void {
    this.clearConfirm();
    this.confirmingEntryId = entryId;
    this.confirmResetHandle = setTimeout(() => this.clearConfirm(), 3500);
  }

  private clearConfirm(): void {
    this.confirmingEntryId = null;
    if (this.confirmResetHandle) {
      clearTimeout(this.confirmResetHandle);
      this.confirmResetHandle = null;
    }
  }
}
