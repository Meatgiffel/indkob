import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  AddFromRecipePayload,
  AuthUser,
  CreateEntryPayload,
  CreateItemPayload,
  GroceryEntry,
  Item,
  MealPlanDay,
  RecipeIngredients,
  RecipeSummary,
  User
} from '../models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getMealPlanWeek(weekStart: string): Observable<MealPlanDay[]> {
    return this.http.get<MealPlanDay[]>(`${this.baseUrl}/mealplan`, { params: { weekStart } });
  }

  upsertMealPlanDay(date: string, payload: Partial<Omit<MealPlanDay, 'date'>>): Observable<MealPlanDay> {
    return this.http.put<MealPlanDay>(`${this.baseUrl}/mealplan/${date}`, payload);
  }

  searchRecipes(q: string): Observable<RecipeSummary[]> {
    return this.http.get<RecipeSummary[]>(`${this.baseUrl}/recipes/search`, { params: { q } });
  }

  getRecipeIngredients(slug: string): Observable<RecipeIngredients> {
    return this.http.get<RecipeIngredients>(`${this.baseUrl}/recipes/${encodeURIComponent(slug)}/ingredients`);
  }

  addRecipeToList(payload: AddFromRecipePayload): Observable<GroceryEntry[]> {
    return this.http.post<GroceryEntry[]>(`${this.baseUrl}/groceryentries/from-recipe`, payload);
  }

  me(): Observable<AuthUser> {
    return this.http.get<AuthUser>(`${this.baseUrl}/auth/me`);
  }

  login(userName: string, password: string, rememberMe: boolean): Observable<AuthUser> {
    return this.http.post<AuthUser>(`${this.baseUrl}/auth/login`, { userName, password, rememberMe });
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

  clearEntries(): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/groceryentries/clear`, {});
  }
}
