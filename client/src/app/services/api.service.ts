import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { CreateEntryPayload, CreateItemPayload, GroceryEntry, Item } from '../models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getItems(): Observable<Item[]> {
    return this.http.get<Item[]>(`${this.baseUrl}/items`);
  }

  createItem(payload: CreateItemPayload): Observable<Item> {
    return this.http.post<Item>(`${this.baseUrl}/items`, payload);
  }

  updateItem(id: number, payload: CreateItemPayload): Observable<Item> {
    return this.http.put<Item>(`${this.baseUrl}/items/${id}`, payload);
  }

  deleteItem(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/items/${id}`);
  }

  getEntries(): Observable<GroceryEntry[]> {
    return this.http.get<GroceryEntry[]>(`${this.baseUrl}/groceryentries`);
  }

  createEntry(payload: CreateEntryPayload): Observable<GroceryEntry> {
    return this.http.post<GroceryEntry>(`${this.baseUrl}/groceryentries`, payload);
  }

  updateEntry(id: number, payload: CreateEntryPayload & { isDone: boolean }): Observable<GroceryEntry> {
    return this.http.put<GroceryEntry>(`${this.baseUrl}/groceryentries/${id}`, payload);
  }

  deleteEntry(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/groceryentries/${id}`);
  }
}
