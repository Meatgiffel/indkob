import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, firstValueFrom } from 'rxjs';

export interface BuildInfo {
  version: string;
  commit?: string;
  builtAtUtc?: string;
}

@Injectable({ providedIn: 'root' })
export class BuildInfoService {
  private loaded = false;
  private readonly buildInfoSubject = new BehaviorSubject<BuildInfo | null>(null);
  buildInfo$ = this.buildInfoSubject.asObservable();

  constructor(private http: HttpClient) {}

  async ensureLoaded(): Promise<void> {
    if (this.loaded) return;
    this.loaded = true;

    try {
      const info = await firstValueFrom(
        this.http.get<BuildInfo>(`/version.json?ts=${Date.now()}`)
      );
      this.buildInfoSubject.next(info);
    } catch {
      this.buildInfoSubject.next(null);
    }
  }
}

