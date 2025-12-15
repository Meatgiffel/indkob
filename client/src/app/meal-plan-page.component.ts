import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { MessageService } from 'primeng/api';

import { ApiService } from './services/api.service';
import { parseHttpError } from './services/http-error';

type MealPlanDay = {
  date: string;
  dinner: string | null;
};

type SaveState = 'idle' | 'dirty' | 'saving' | 'saved' | 'error';

type DaySaveStatus = {
  state: SaveState;
  dirty: boolean;
  lastSavedNormalized: string;
  debounceHandle: ReturnType<typeof setTimeout> | null;
  savingIconDelayHandle: ReturnType<typeof setTimeout> | null;
  showSavingIcon: boolean;
  resetHandle: ReturnType<typeof setTimeout> | null;
};

@Component({
  selector: 'app-meal-plan-page',
  standalone: true,
  imports: [CommonModule, FormsModule, CardModule, ButtonModule, InputTextareaModule],
  templateUrl: './meal-plan-page.component.html',
  styleUrl: './meal-plan-page.component.scss'
})
export class MealPlanPageComponent implements OnInit {
  loading = false;
  weekStart = startOfWeek(new Date());
  days: MealPlanDay[] = [];
  private saving = new Set<string>();
  private readonly statusByDate = new Map<string, DaySaveStatus>();

  constructor(
    private api: ApiService,
    private toast: MessageService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  get weekLabel(): string {
    const start = parseIsoDate(this.weekStart);
    const end = new Date(start.getTime());
    end.setDate(end.getDate() + 6);
    const weekNo = isoWeekNumber(start);
    return `Uge ${weekNo} (${formatShortDate(start)} - ${formatShortDate(end)})`;
  }

  isSaving(date: string): boolean {
    return this.saving.has(date);
  }

  saveState(date: string): SaveState {
    const status = this.getStatus(date);
    if (status.state !== 'saving') return status.state;
    return status.showSavingIcon ? 'saving' : 'dirty';
  }

  async load(): Promise<void> {
    this.loading = true;
    try {
      const response = await firstValueFrom(this.api.getMealPlanWeek(this.weekStart));
      this.resetStatuses(response);
      this.days = response;
    } catch (err: any) {
      this.toast.add({ severity: 'error', summary: 'Fejl', detail: parseHttpError(err, 'Kunne ikke hente madplan.') });
    } finally {
      this.loading = false;
    }
  }

  async previousWeek(): Promise<void> {
    const date = parseIsoDate(this.weekStart);
    date.setDate(date.getDate() - 7);
    this.weekStart = startOfWeek(date);
    await this.load();
  }

  async nextWeek(): Promise<void> {
    const date = parseIsoDate(this.weekStart);
    date.setDate(date.getDate() + 7);
    this.weekStart = startOfWeek(date);
    await this.load();
  }

  async goToCurrentWeek(): Promise<void> {
    this.weekStart = startOfWeek(new Date());
    await this.load();
  }

  dayShort(dateIso: string): string {
    const date = parseIsoDate(dateIso);
    const day = (date.getDay() + 6) % 7; // Monday=0
    const names = ['Man', 'Tir', 'Ons', 'Tor', 'Fre', 'Lør', 'Søn'];
    return names[day] ?? '';
  }

  dayDate(dateIso: string): string {
    const date = parseIsoDate(dateIso);
    return formatShortDate(date);
  }

  onDinnerChange(day: MealPlanDay): void {
    const status = this.getStatus(day.date);
    status.dirty = true;
    status.state = 'dirty';
    this.clearResetHandle(status);
    this.scheduleSave(day);
  }

  async saveDay(day: MealPlanDay): Promise<void> {
    if (this.isSaving(day.date)) {
      this.scheduleSave(day);
      return;
    }

    const status = this.getStatus(day.date);
    this.clearDebounceHandle(status);

    const dinner = (day.dinner ?? '').trim();
    if (!status.dirty && dinner === status.lastSavedNormalized) return;
    if (dinner === status.lastSavedNormalized) {
      status.dirty = false;
      status.state = 'saved';
      this.scheduleResetSaved(day.date);
      return;
    }

    status.state = 'saving';
    status.showSavingIcon = false;
    this.clearSavingIconDelayHandle(status);
    status.savingIconDelayHandle = setTimeout(() => {
      const latest = this.statusByDate.get(day.date);
      if (latest?.state === 'saving') latest.showSavingIcon = true;
    }, 250);

    this.saving.add(day.date);
    try {
      const updated = await firstValueFrom(this.api.upsertMealPlanDay(day.date, dinner || null));
      day.dinner = updated.dinner;
      status.lastSavedNormalized = (updated.dinner ?? '').trim();
      status.dirty = false;
      status.state = 'saved';
      status.showSavingIcon = false;
      this.clearSavingIconDelayHandle(status);
      this.scheduleResetSaved(day.date);
    } catch (err: any) {
      this.toast.add({ severity: 'error', summary: 'Fejl', detail: parseHttpError(err, 'Kunne ikke gemme madplan.') });
      status.state = 'error';
      status.showSavingIcon = false;
      this.clearSavingIconDelayHandle(status);
    } finally {
      this.saving.delete(day.date);
    }
  }

  async clearDay(day: MealPlanDay): Promise<void> {
    const status = this.getStatus(day.date);
    this.clearDebounceHandle(status);
    this.clearResetHandle(status);
    status.dirty = true;
    day.dinner = null;
    await this.saveDay(day);
  }

  private scheduleSave(day: MealPlanDay): void {
    const status = this.getStatus(day.date);
    this.clearDebounceHandle(status);
    status.debounceHandle = setTimeout(() => void this.saveDay(day), 900);
  }

  private resetStatuses(days: MealPlanDay[]): void {
    for (const status of this.statusByDate.values()) {
      this.clearDebounceHandle(status);
      this.clearSavingIconDelayHandle(status);
      this.clearResetHandle(status);
    }
    this.statusByDate.clear();
    this.saving.clear();

    for (const day of days) {
      this.statusByDate.set(day.date, {
        state: 'idle',
        dirty: false,
        lastSavedNormalized: (day.dinner ?? '').trim(),
        debounceHandle: null,
        savingIconDelayHandle: null,
        showSavingIcon: false,
        resetHandle: null
      });
    }
  }

  private getStatus(date: string): DaySaveStatus {
    const existing = this.statusByDate.get(date);
    if (existing) return existing;

    const created: DaySaveStatus = {
      state: 'idle',
      dirty: false,
      lastSavedNormalized: '',
      debounceHandle: null,
      savingIconDelayHandle: null,
      showSavingIcon: false,
      resetHandle: null
    };
    this.statusByDate.set(date, created);
    return created;
  }

  private scheduleResetSaved(date: string): void {
    const status = this.getStatus(date);
    this.clearResetHandle(status);
    status.resetHandle = setTimeout(() => {
      const latest = this.statusByDate.get(date);
      if (latest && latest.state === 'saved') latest.state = 'idle';
    }, 2000);
  }

  private clearDebounceHandle(status: DaySaveStatus): void {
    if (!status.debounceHandle) return;
    clearTimeout(status.debounceHandle);
    status.debounceHandle = null;
  }

  private clearResetHandle(status: DaySaveStatus): void {
    if (!status.resetHandle) return;
    clearTimeout(status.resetHandle);
    status.resetHandle = null;
  }

  private clearSavingIconDelayHandle(status: DaySaveStatus): void {
    if (!status.savingIconDelayHandle) return;
    clearTimeout(status.savingIconDelayHandle);
    status.savingIconDelayHandle = null;
  }
}

function startOfWeek(date: Date): string {
  const d = new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate()));
  const day = d.getUTCDay(); // Sunday=0
  const diff = (day + 6) % 7; // Monday=0
  d.setUTCDate(d.getUTCDate() - diff);
  return toIsoDate(d);
}

function toIsoDate(date: Date): string {
  const year = date.getUTCFullYear();
  const month = String(date.getUTCMonth() + 1).padStart(2, '0');
  const day = String(date.getUTCDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

function parseIsoDate(iso: string): Date {
  const [y, m, d] = iso.split('-').map(Number);
  return new Date(y, (m ?? 1) - 1, d ?? 1);
}

function formatShortDate(date: Date): string {
  return date.toLocaleDateString('da-DK', { day: '2-digit', month: '2-digit' });
}

function isoWeekNumber(dateLocal: Date): number {
  const date = new Date(Date.UTC(dateLocal.getFullYear(), dateLocal.getMonth(), dateLocal.getDate()));
  const day = date.getUTCDay() || 7;
  date.setUTCDate(date.getUTCDate() + 4 - day);
  const yearStart = new Date(Date.UTC(date.getUTCFullYear(), 0, 1));
  const weekNo = Math.ceil((((date.getTime() - yearStart.getTime()) / 86400000) + 1) / 7);
  return weekNo;
}
