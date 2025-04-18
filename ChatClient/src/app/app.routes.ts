import {Routes} from '@angular/router';
import {loginGuard} from './guards/login.guard';
import {ChatComponent} from './chat/chat.component';
import {authGuard} from './guards/auth.guard';


export const routes: Routes = [
  {
    path: "chat",
    component: ChatComponent,
    canActivate: [authGuard]
  },

  {
    path: 'register',
    loadComponent: () => import('./register/register.component').then(r => r.RegisterComponent),
    canActivate: [loginGuard]

  },
  {
    path: 'login',
    loadComponent: () => import('./login/login.component').then(l => l.LoginComponent),
    canActivate: [loginGuard]
  },
  {
    path: '**',
    redirectTo: 'chat',
    pathMatch: 'full'

  }


];
