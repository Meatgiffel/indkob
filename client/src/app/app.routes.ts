import { Routes } from '@angular/router';
import { ItemsPageComponent } from './items-page.component';
import { ListPageComponent } from './list-page.component';
import { ShopPageComponent } from './shop-page.component';
import { LoginPageComponent } from './login-page.component';
import { UsersPageComponent } from './users-page.component';
import { authGuard } from './services/auth.guard';
import { adminGuard } from './services/admin.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'list' },
  { path: 'login', component: LoginPageComponent },
  { path: 'list', component: ListPageComponent, canActivate: [authGuard] },
  { path: 'shop', component: ShopPageComponent, canActivate: [authGuard] },
  { path: 'items', component: ItemsPageComponent, canActivate: [authGuard] },
  { path: 'users', component: UsersPageComponent, canActivate: [authGuard, adminGuard] },
  { path: '**', redirectTo: 'list' }
];
