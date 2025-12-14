import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { CheckboxModule } from 'primeng/checkbox';
import { MessageService } from 'primeng/api';

import { ApiService } from './services/api.service';
import { parseHttpError } from './services/http-error';
import { User } from './models';

@Component({
  selector: 'app-users-page',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CardModule,
    ButtonModule,
    TableModule,
    DialogModule,
    InputTextModule,
    PasswordModule,
    CheckboxModule
  ],
  templateUrl: './users-page.component.html',
  styleUrl: './users-page.component.scss'
})
export class UsersPageComponent implements OnInit {
  users: User[] = [];
  loading = false;

  dialogOpen = false;
  editing: User | null = null;
  saving = false;

  form = this.fb.group({
    userName: ['', Validators.required],
    password: [''],
    isAdmin: [false]
  });

  constructor(
    private fb: FormBuilder,
    private api: ApiService,
    private toast: MessageService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  async load(): Promise<void> {
    this.loading = true;
    try {
      this.users = await firstValueFrom(this.api.getUsers());
    } catch (err) {
      this.toast.add({ severity: 'error', summary: 'Fejl', detail: 'Kunne ikke hente brugere.' });
    } finally {
      this.loading = false;
    }
  }

  openCreate(): void {
    this.editing = null;
    this.form.reset({ userName: '', password: '', isAdmin: false });
    this.dialogOpen = true;
  }

  openEdit(user: User): void {
    this.editing = user;
    this.form.reset({ userName: user.userName, password: '', isAdmin: user.isAdmin });
    this.dialogOpen = true;
  }

  closeDialog(): void {
    this.dialogOpen = false;
    this.editing = null;
    this.form.reset({ userName: '', password: '', isAdmin: false });
  }

  async save(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const userName = this.form.value.userName!.trim();
    const password = (this.form.value.password ?? '').trim();
    const isAdmin = !!this.form.value.isAdmin;

    this.saving = true;
    try {
      if (this.editing) {
        await firstValueFrom(
          this.api.updateUser(this.editing.id, { userName, password: password || null, isAdmin })
        );
        this.toast.add({ severity: 'success', summary: 'Opdateret', detail: 'Bruger opdateret.' });
      } else {
        if (!password) {
          this.toast.add({ severity: 'warn', summary: 'Manglende', detail: 'Password er påkrævet.' });
          return;
        }
        await firstValueFrom(this.api.createUser({ userName, password, isAdmin }));
        this.toast.add({ severity: 'success', summary: 'Oprettet', detail: 'Bruger oprettet.' });
      }

      await this.load();
      this.closeDialog();
    } catch (err: any) {
      this.toast.add({ severity: 'error', summary: 'Fejl', detail: parseHttpError(err, 'Noget gik galt.') });
    } finally {
      this.saving = false;
    }
  }

  async delete(user: User): Promise<void> {
    const ok = window.confirm(`Slet bruger "${user.userName}"?`);
    if (!ok) return;

    try {
      await firstValueFrom(this.api.deleteUser(user.id));
      this.toast.add({ severity: 'success', summary: 'Slettet', detail: 'Bruger slettet.' });
      await this.load();
    } catch (err: any) {
      this.toast.add({ severity: 'warn', summary: 'Kunne ikke slette', detail: parseHttpError(err, 'Kunne ikke slette.') });
    }
  }
}

