import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { CardModule } from 'primeng/card';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { MessageService } from 'primeng/api';

import { ApiService } from './services/api.service';
import { parseHttpError } from './services/http-error';
import { Item } from './models';

@Component({
  selector: 'app-items-page',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    CardModule,
    AutoCompleteModule,
    InputTextModule,
    ButtonModule,
    TableModule,
    TagModule,
    DividerModule
  ],
  templateUrl: './items-page.component.html',
  styleUrl: './items-page.component.scss'
})
export class ItemsPageComponent implements OnInit {
  items: Item[] = [];
  areaOptions: string[] = [];
  filteredAreas: string[] = [];
  loadingItems = false;
  savingItem = false;
  rowEditingId: number | null = null;
  editDraft = { name: '', area: '' };

  itemForm = this.fb.group({
    name: ['', Validators.required],
    area: ['', Validators.required]
  });

  constructor(
    private fb: FormBuilder,
    private api: ApiService,
    private toast: MessageService
  ) {}

  ngOnInit(): void {
    this.loadItems();
  }

  async loadItems(): Promise<void> {
    this.loadingItems = true;
    try {
      this.items = await firstValueFrom(this.api.getItems());
      this.updateAreaOptions();
    } catch (err) {
      this.toast.add({ severity: 'error', summary: 'Fejl', detail: 'Kunne ikke hente varer.' });
    } finally {
      this.loadingItems = false;
    }
  }

  async saveItem(): Promise<void> {
    if (this.itemForm.invalid) {
      this.itemForm.markAllAsTouched();
      return;
    }

    this.savingItem = true;
    const payload = this.itemForm.value;
    try {
      await firstValueFrom(
        this.api.createItem({
          name: payload.name!.trim(),
          area: payload.area!.trim()
        })
      );
      this.toast.add({ severity: 'success', summary: 'Tilføjet', detail: 'Ny vare oprettet.' });
      await this.loadItems();
      this.itemForm.reset({ name: '', area: '' });
    } catch (err: any) {
      this.toast.add({ severity: 'error', summary: 'Fejl', detail: parseHttpError(err, 'Noget gik galt.') });
    } finally {
      this.savingItem = false;
    }
  }

  async deleteItem(item: Item): Promise<void> {
    const confirmation = window.confirm(`Slet varen "${item.name}"?`);
    if (!confirmation) {
      return;
    }
    try {
      await firstValueFrom(this.api.deleteItem(item.id));
      this.toast.add({ severity: 'success', summary: 'Slettet', detail: 'Varen blev fjernet.' });
      await this.loadItems();
    } catch (err: any) {
      this.toast.add({
        severity: 'warn',
        summary: 'Kunne ikke slette',
        detail: parseHttpError(err, 'Varen kunne ikke slettes.')
      });
    }
  }

  filterAreas(event: any): void {
    const query = (event.query ?? '').toLowerCase();
    this.filteredAreas = this.areaOptions.filter(area => area.toLowerCase().includes(query));
  }

  beginInlineEdit(item: Item, focusArea = false): void {
    this.rowEditingId = item.id;
    this.editDraft = { name: item.name, area: item.area };
    if (focusArea) {
      setTimeout(() => document.getElementById(`inline-area-${item.id}`)?.focus(), 0);
    }
  }

  cancelInlineEdit(): void {
    this.rowEditingId = null;
    this.editDraft = { name: '', area: '' };
  }

  async saveInlineEdit(item: Item): Promise<void> {
    const name = this.editDraft.name.trim();
    const area = this.editDraft.area.trim();
    if (!name || !area) {
      this.toast.add({ severity: 'warn', summary: 'Manglende data', detail: 'Navn og område er påkrævet.' });
      return;
    }
    this.savingItem = true;
    try {
      await firstValueFrom(
        this.api.updateItem(item.id, {
          name,
          area
        })
      );
      this.toast.add({ severity: 'success', summary: 'Opdateret', detail: 'Vare opdateret.' });
      await this.loadItems();
      this.cancelInlineEdit();
    } catch (err: any) {
      this.toast.add({ severity: 'error', summary: 'Fejl', detail: parseHttpError(err, 'Noget gik galt.') });
    } finally {
      this.savingItem = false;
    }
  }

  private updateAreaOptions(): void {
    this.areaOptions = Array.from(new Set(this.items.map(item => item.area).filter(Boolean))).sort();
  }
}
