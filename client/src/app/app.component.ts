import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ToolbarModule } from 'primeng/toolbar';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { MessageService, PrimeNGConfig } from 'primeng/api';
import { AuthService } from './services/auth.service';
import { filter } from 'rxjs';
import { BuildInfoService } from './services/build-info.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, ToolbarModule, TagModule, ButtonModule, ToastModule],
  providers: [MessageService],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  showChrome = true;

  constructor(
    private primengConfig: PrimeNGConfig,
    public auth: AuthService,
    public buildInfo: BuildInfoService,
    private router: Router
  ) {}

  async ngOnInit(): Promise<void> {
    this.primengConfig.ripple = true;
    await this.auth.ensureLoaded();
    await this.buildInfo.ensureLoaded();

    this.updateChromeVisibility(this.router.url);
    this.router.events.pipe(filter(event => event instanceof NavigationEnd)).subscribe(event => {
      this.updateChromeVisibility((event as NavigationEnd).urlAfterRedirects);
    });
  }

  async logout(): Promise<void> {
    await this.auth.logout();
    await this.router.navigateByUrl('/login');
  }

  private updateChromeVisibility(url: string): void {
    this.showChrome = !url.startsWith('/login');
  }
}
