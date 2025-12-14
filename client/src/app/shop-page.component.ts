import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { CardModule } from 'primeng/card';
import { DropdownModule } from 'primeng/dropdown';
import { ButtonModule } from 'primeng/button';
import { MessageService } from 'primeng/api';

import { ApiService } from './services/api.service';
import { GroceryEntry } from './models';
import { parseHttpError } from './services/http-error';

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
  private updating = new Set<number>();

  constructor(
    private api: ApiService,
    private toast: MessageService
  ) {}

  ngOnInit(): void {
    this.loadEntries();
  }

  get openCount(): number {
    return this.entries.filter(e => !e.isDone).length;
  }

  get doneCount(): number {
    return this.entries.filter(e => e.isDone).length;
  }

  private get visibleEntries(): GroceryEntry[] {
    if (this.doneFilter === 'done') return this.entries.filter(e => e.isDone);
    if (this.doneFilter === 'open') return this.entries.filter(e => !e.isDone);
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
    this.loadingEntries = true;
    try {
      this.entries = await firstValueFrom(this.api.getEntries());
    } catch (err: any) {
      this.toast.add({ severity: 'error', summary: 'Fejl', detail: parseHttpError(err, 'Kunne ikke hente indkøbsseddel.') });
    } finally {
      this.loadingEntries = false;
    }
  }

  isUpdating(entry: GroceryEntry): boolean {
    return this.updating.has(entry.id);
  }

  async toggleDone(entry: GroceryEntry): Promise<void> {
    if (this.isUpdating(entry)) return;

    const previous = entry.isDone;
    entry.isDone = !previous;
    this.updating.add(entry.id);

    try {
      await firstValueFrom(
        this.api.updateEntry(entry.id, {
          itemId: entry.itemId,
          amount: entry.amount ?? null,
          note: entry.note ?? null,
          isDone: entry.isDone
        })
      );
    } catch (err: any) {
      entry.isDone = previous;
      this.toast.add({ severity: 'error', summary: 'Fejl', detail: parseHttpError(err, 'Kunne ikke opdatere status.') });
    } finally {
      this.updating.delete(entry.id);
    }
  }
}
