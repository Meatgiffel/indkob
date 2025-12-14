import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthUser, CreateEntryPayload, CreateItemPayload, GroceryEntry, Item, User } from '../models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getMealPlanWeek(weekStart: string): Observable<{ date: string; dinner: string | null }[]> {
    return this.http.get<{ date: string; dinner: string | null }[]>(`${this.baseUrl}/mealplan`, { params: { weekStart } });
  }

  upsertMealPlanDay(date: string, dinner: string | null): Observable<{ date: string; dinner: string | null }> {
    return this.http.put<{ date: string; dinner: string | null }>(`${this.baseUrl}/mealplan/${date}`, { dinner });
  }

  me(): Observable<AuthUser> {
    return this.http.get<AuthUser>(`${this.baseUrl}/auth/me`);
  }

  login(userName: string, password: string): Observable<AuthUser> {
    return this.http.post<AuthUser>(`${this.baseUrl}/auth/login`, { userName, password });
  }

  logout(): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/auth/logout`, {});
  }

  getUsers(): Observable<User[]> {
    return this.http.get<User[]>(`${this.baseUrl}/users`);
  }

  createUser(payload: { userName: string; password: string; isAdmin: boolean }): Observable<User> {
    return this.http.post<User>(`${this.baseUrl}/users`, payload);
  }

  updateUser(id: number, payload: { userName: string; password?: string | null; isAdmin: boolean }): Observable<User> {
    return this.http.put<User>(`${this.baseUrl}/users/${id}`, payload);
  }

  deleteUser(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/users/${id}`);
  }

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
