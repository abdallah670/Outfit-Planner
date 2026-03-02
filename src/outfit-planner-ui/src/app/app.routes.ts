import { Routes } from '@angular/router';
import { Login } from './presentation/pages/auth/login/login';
import { Register } from './presentation/pages/auth/register/register';
import { WardrobeDashboardComponent } from './presentation/pages/wardrobe-dashboard/wardrobe-dashboard.component';
import { AddClothingItemComponent } from './presentation/pages/add-clothing-item/add-clothing-item.component';
import { ClothingItemDetail } from './presentation/pages/clothing-item-detail/clothing-item-detail';
import { HomeComponent } from './presentation/pages/home/home.component';
import { authGuard } from './core/guards/auth-guard';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'home', redirectTo: '', pathMatch: 'full' },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'wardrobe', component: WardrobeDashboardComponent, canActivate: [authGuard] },
  { path: 'wardrobe/new', component: AddClothingItemComponent, canActivate: [authGuard] },
  { path: 'wardrobe/edit/:id', component: AddClothingItemComponent, canActivate: [authGuard] },
  { path: 'wardrobe/:id', component: ClothingItemDetail, canActivate: [authGuard] },
];
