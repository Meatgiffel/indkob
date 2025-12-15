import { Injectable } from '@angular/core';
import { BehaviorSubject, firstValueFrom } from 'rxjs';
import { ApiService } from './api.service';
import { AuthUser } from '../models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private checked = false;
  private readonly userSubject = new BehaviorSubject<AuthUser | null>(null);

  user$ = this.userSubject.asObservable();

  constructor(private api: ApiService) {}

  get snapshot(): AuthUser | null {
    return this.userSubject.value;
  }

  get isAuthenticated(): boolean {
    return !!this.userSubject.value;
  }

  async ensureLoaded(): Promise<void> {
    if (this.checked) return;
    this.checked = true;
    try {
      const user = await firstValueFrom(this.api.me());
      this.userSubject.next(user);
    } catch {
      this.userSubject.next(null);
    }
  }

  async login(userName: string, password: string, rememberMe: boolean): Promise<AuthUser> {
    const user = await firstValueFrom(this.api.login(userName, password, rememberMe));
    this.userSubject.next(user);
    return user;
  }

  async logout(): Promise<void> {
    try {
      await firstValueFrom(this.api.logout());
    } finally {
      this.userSubject.next(null);
      this.checked = true;
    }
  }
}
