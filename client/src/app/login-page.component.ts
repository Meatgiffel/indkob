import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { MessageService } from 'primeng/api';
import { AuthService } from './services/auth.service';
import { parseHttpError } from './services/http-error';
import { BuildInfoService } from './services/build-info.service';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, CardModule, InputTextModule, PasswordModule, ButtonModule, CheckboxModule],
  templateUrl: './login-page.component.html',
  styleUrl: './login-page.component.scss'
})
export class LoginPageComponent implements OnInit {
  loading = false;
  private returnUrl: string | null = null;

  form = this.fb.group({
    userName: ['', Validators.required],
    password: ['', Validators.required],
    rememberMe: [false]
  });

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private toast: MessageService,
    public buildInfo: BuildInfoService
  ) {}

  async ngOnInit(): Promise<void> {
    await this.auth.ensureLoaded();
    await this.buildInfo.ensureLoaded();
    this.returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
    if (this.auth.isAuthenticated) {
      await this.navigateAfterLogin();
    }
  }

  async submit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const userName = this.form.value.userName!.trim();
    const password = this.form.value.password!;
    const rememberMe = !!this.form.value.rememberMe;

    this.loading = true;
    try {
      await this.auth.login(userName, password, rememberMe);
      await this.navigateAfterLogin();
    } catch (err: any) {
      this.toast.add({ severity: 'error', summary: 'Login fejlede', detail: parseHttpError(err, 'Forkert login.') });
    } finally {
      this.loading = false;
    }
  }

  private async navigateAfterLogin(): Promise<void> {
    const target = this.returnUrl && this.returnUrl.startsWith('/') ? this.returnUrl : '/list';
    await this.router.navigateByUrl(target);
  }
}
