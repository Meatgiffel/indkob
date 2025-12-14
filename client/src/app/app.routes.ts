import { Routes } from '@angular/router';
import { ItemsPageComponent } from './items-page.component';
import { ListPageComponent } from './list-page.component';
import { ShopPageComponent } from './shop-page.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'list' },
  { path: 'list', component: ListPageComponent },
  { path: 'shop', component: ShopPageComponent },
  { path: 'items', component: ItemsPageComponent },
  { path: '**', redirectTo: 'list' }
];
