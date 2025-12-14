import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { MessageService } from 'primeng/api';

import { ApiService } from './services/api.service';
import { parseHttpError } from './services/http-error';

type MealPlanDay = {
  date: string;
  dinner: string | null;
};

@Component({
  selector: 'app-meal-plan-page',
  standalone: true,
  imports: [CommonModule, FormsModule, CardModule, ButtonModule, InputTextModule],
  templateUrl: './meal-plan-page.component.html',
  styleUrl: './meal-plan-page.component.scss'
})
export class MealPlanPageComponent implements OnInit {
  loading = false;
  weekStart = startOfWeek(new Date());
  days: MealPlanDay[] = [];
  private saving = new Set<string>();

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

  async load(): Promise<void> {
    this.loading = true;
    try {
      const response = await firstValueFrom(this.api.getMealPlanWeek(this.weekStart));
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

  dayLabel(dateIso: string): string {
    const date = parseIsoDate(dateIso);
    const day = (date.getDay() + 6) % 7; // Monday=0
    const names = ['Mandag', 'Tirsdag', 'Onsdag', 'Torsdag', 'Fredag', 'Lørdag', 'Søndag'];
    return `${names[day]} (${formatShortDate(date)})`;
  }

  async saveDay(day: MealPlanDay): Promise<void> {
    if (this.isSaving(day.date)) return;

    const dinner = (day.dinner ?? '').trim();
    this.saving.add(day.date);
    try {
      const updated = await firstValueFrom(this.api.upsertMealPlanDay(day.date, dinner || null));
      day.dinner = updated.dinner;
    } catch (err: any) {
      this.toast.add({ severity: 'error', summary: 'Fejl', detail: parseHttpError(err, 'Kunne ikke gemme madplan.') });
    } finally {
      this.saving.delete(day.date);
    }
  }

  async clearDay(day: MealPlanDay): Promise<void> {
    day.dinner = null;
    await this.saveDay(day);
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

