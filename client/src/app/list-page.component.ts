import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { DropdownModule } from 'primeng/dropdown';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { CheckboxModule } from 'primeng/checkbox';
import { DividerModule } from 'primeng/divider';
import { MessageService } from 'primeng/api';

import { ApiService } from './services/api.service';
import { parseHttpError } from './services/http-error';
import { GroceryEntry, Item } from './models';

@Component({
  selector: 'app-list-page',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    DropdownModule,
    AutoCompleteModule,
    InputTextModule,
    ButtonModule,
    CardModule,
    TagModule,
    CheckboxModule,
    DividerModule
  ],
  templateUrl: './list-page.component.html',
  styleUrl: './list-page.component.scss'
})
export class ListPageComponent implements OnInit {
  items: Item[] = [];
  entries: GroceryEntry[] = [];
  filteredItems: (Item & { _create?: boolean })[] = [];
  areaOptions: string[] = [];
  filteredAreas: string[] = [];
  doneFilter: 'all' | 'done' | 'open' = 'all';
  readonly doneFilterOptions = [
    { label: 'Alle', value: 'all' as const },
    { label: 'Færdige', value: 'done' as const },
    { label: 'Mangler', value: 'open' as const }
  ];
  searchTerm = '';
  typedTerm = '';
  lastSelectedTerm = '';
  newItemArea = '';
  creatingItem = false;
  loadingItems = false;
  loadingEntries = false;
  savingEntry = false;
  editingEntryId: number | null = null;

  entryForm = this.fb.group({
    itemId: [null as number | null, Validators.required],
    amount: [''],
    note: [''],
    isDone: [false]
  });

  constructor(
    private fb: FormBuilder,
    private api: ApiService,
    private toast: MessageService
  ) {}

  ngOnInit(): void {
    this.loadItems();
    this.loadEntries();
  }

  async loadItems(): Promise<void> {
    this.loadingItems = true;
    try {
      this.items = await firstValueFrom(this.api.getItems());
      this.filteredItems = this.items.slice(0, 8);
      this.updateAreaOptions();
    } catch (err) {
      this.toast.add({ severity: 'error', summary: 'Fejl', detail: 'Kunne ikke hente varer.' });
    } finally {
      this.loadingItems = false;
    }
  }

  async loadEntries(): Promise<void> {
    this.loadingEntries = true;
    try {
      this.entries = await firstValueFrom(this.api.getEntries());
    } catch (err) {
      this.toast.add({ severity: 'error', summary: 'Fejl', detail: 'Kunne ikke hente indkøbsseddel.' });
    } finally {
      this.loadingEntries = false;
    }
  }

  startEditEntry(entry: GroceryEntry): void {
    this.editingEntryId = entry.id;
    this.entryForm.patchValue({
      itemId: entry.itemId,
      amount: entry.amount ?? '',
      note: entry.note ?? '',
      isDone: entry.isDone
    });
    this.searchTerm = entry.itemName;
    this.lastSelectedTerm = entry.itemName;
  }

  cancelEntryEdit(): void {
    this.editingEntryId = null;
    this.entryForm.reset({ itemId: null, amount: '', note: '', isDone: false });
    this.searchTerm = '';
    this.lastSelectedTerm = '';
  }

  async saveEntry(): Promise<void> {
    if (this.entryForm.invalid) {
      this.entryForm.markAllAsTouched();
      return;
    }

    this.savingEntry = true;
    const payload = this.entryForm.value;
    try {
      if (this.editingEntryId) {
        await firstValueFrom(
          this.api.updateEntry(this.editingEntryId, {
            itemId: payload.itemId!,
            amount: (payload.amount ?? '').trim(),
            note: (payload.note ?? '').trim(),
            isDone: payload.isDone ?? false
          })
        );
        this.toast.add({ severity: 'success', summary: 'Opdateret', detail: 'Indkøbslinje opdateret.' });
      } else {
        await firstValueFrom(
          this.api.createEntry({
            itemId: payload.itemId!,
            amount: (payload.amount ?? '').trim(),
            note: (payload.note ?? '').trim()
          })
        );
        this.toast.add({ severity: 'success', summary: 'Tilføjet', detail: 'Vare tilføjet til listen.' });
      }
      await this.loadEntries();
      this.cancelEntryEdit();
    } catch (err: any) {
      this.toast.add({ severity: 'error', summary: 'Fejl', detail: parseHttpError(err, 'Noget gik galt.') });
    } finally {
      this.savingEntry = false;
    }
  }

  async toggleDone(entry: GroceryEntry): Promise<void> {
    try {
      await firstValueFrom(
        this.api.updateEntry(entry.id, {
          itemId: entry.itemId,
          amount: entry.amount ?? '',
          note: entry.note ?? '',
          isDone: !entry.isDone
        })
      );
      await this.loadEntries();
    } catch {
      this.toast.add({ severity: 'error', summary: 'Fejl', detail: 'Kunne ikke opdatere status.' });
    }
  }

  async deleteEntry(entry: GroceryEntry): Promise<void> {
    const confirmation = window.confirm(`Fjern "${entry.itemName}" fra listen?`);
    if (!confirmation) {
      return;
    }
    try {
      await firstValueFrom(this.api.deleteEntry(entry.id));
      this.toast.add({ severity: 'success', summary: 'Fjernet', detail: 'Linjen blev fjernet.' });
      await this.loadEntries();
    } catch {
      this.toast.add({ severity: 'error', summary: 'Fejl', detail: 'Kunne ikke slette linjen.' });
    }
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

  get groupedEntries(): { area: string; entries: GroceryEntry[]; openCount: number; doneCount: number }[] {
    const groups = new Map<string, GroceryEntry[]>();
    for (const entry of this.visibleEntries) {
      const key = entry.itemArea || 'Andet';
      if (!groups.has(key)) {
        groups.set(key, []);
      }
      groups.get(key)!.push(entry);
    }

    return Array.from(groups.entries())
      .sort((a, b) => a[0].localeCompare(b[0], 'da', { sensitivity: 'base' }))
      .map(([area, entries]) => {
        const sorted = [...entries].sort(
          (a, b) => Number(a.isDone) - Number(b.isDone) || a.itemName.localeCompare(b.itemName, 'da', { sensitivity: 'base' })
        );
        const openCount = sorted.filter(e => !e.isDone).length;
        return { area, entries: sorted, openCount, doneCount: sorted.length - openCount };
      });
  }

  filterAreas(event: any): void {
    const query = (event.query ?? '').toLowerCase();
    this.filteredAreas = this.areaOptions.filter(area => area.toLowerCase().includes(query));
  }

  filterItems(event: any): void {
    const queryRaw = (event.query ?? '').trim();
    const query = queryRaw.toLowerCase();
    const base = this.items.filter(
      item => item.name.toLowerCase().includes(query) || (item.area ?? '').toLowerCase().includes(query)
    );

    const alreadyExists = this.items.some(item => item.name.toLowerCase() === query);
    const createOption: (Item & { _create?: boolean })[] =
      queryRaw && !alreadyExists
        ? [
            {
              id: -1,
              name: `Opret "${queryRaw}"`,
              area: this.newItemArea || this.filteredAreas[0] || 'Andet',
              _create: true
            }
          ]
        : [];

    this.filteredItems = [...base.slice(0, 7), ...createOption];
  }

  selectItem(event: any): void {
    const item = event?.value as (Item & { _create?: boolean }) | undefined;
    if (!item) return;
    if (item._create) {
      this.createItemFromSearch();
      return;
    }
    this.entryForm.patchValue({ itemId: item.id });
    this.searchTerm = item.name;
    this.lastSelectedTerm = item.name;
    this.typedTerm = item.name;
  }

  onSearchChange(term: string): void {
    this.searchTerm = term;
    this.typedTerm = term;
    if (!term) {
      this.entryForm.patchValue({ itemId: null });
      this.filteredItems = this.items.slice(0, 8);
      this.lastSelectedTerm = '';
      this.newItemArea = '';
    } else if (term !== this.lastSelectedTerm) {
      this.entryForm.patchValue({ itemId: null });
    }
  }

  maybeCreateFromEnter(event: KeyboardEvent | Event): void {
    const key = (event as KeyboardEvent)?.key;
    if (key === 'Enter' && !this.entryForm.value.itemId && (this.typedTerm.trim() || this.searchTerm.trim())) {
      event.preventDefault();
      this.createItemFromSearch();
    }
  }

  async createItemFromSearch(): Promise<void> {
    const name = (this.typedTerm || this.searchTerm).trim();
    const area = (this.newItemArea || this.filteredAreas[0] || 'Andet').trim();
    if (!name) {
      this.toast.add({ severity: 'warn', summary: 'Manglende data', detail: 'Navn er påkrævet.' });
      return;
    }
    this.creatingItem = true;
    try {
      const newItem = await firstValueFrom(
        this.api.createItem({
          name,
          area
        })
      );
      this.items.unshift(newItem);
      this.updateAreaOptions();
      this.filteredItems = this.items.slice(0, 8);
      this.entryForm.patchValue({ itemId: newItem.id });
      this.searchTerm = newItem.name;
      this.lastSelectedTerm = newItem.name;
      this.newItemArea = '';
      this.toast.add({ severity: 'success', summary: 'Tilføjet', detail: `Oprettede "${newItem.name}".` });
    } catch (err: any) {
      this.toast.add({ severity: 'error', summary: 'Fejl', detail: parseHttpError(err, 'Kunne ikke oprette varen.') });
    } finally {
      this.creatingItem = false;
    }
  }

  private updateAreaOptions(): void {
    this.areaOptions = Array.from(new Set(this.items.map(item => item.area).filter(Boolean))).sort();
    this.filteredAreas = this.areaOptions.slice(0, 8);
  }
}
