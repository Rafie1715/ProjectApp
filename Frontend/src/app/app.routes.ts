import { inject } from '@angular/core';
import { CanActivateFn, Router, Routes } from '@angular/router';
import { ApprovalComponent } from './pages/approval/approval.component';
import { CutiDetailsComponent } from './pages/cuti-details/cuti-details.component';
import { HomeComponent } from './pages/home/home.component';
import { LoginComponent } from './pages/login/login.component';
import { AuthService } from './services/auth.service';

const signedIn: CanActivateFn = () =>
  !!inject(AuthService).token || inject(Router).parseUrl('/login');

const enterLogin: CanActivateFn = () => {
  const auth = inject(AuthService);
  return !auth.token || inject(Router).parseUrl(auth.landingPage);
};

export const routes: Routes = [
  { path: 'login', component: LoginComponent, canActivate: [enterLogin], title: 'Login | CutiApp' },
  { path: 'home', component: HomeComponent, canActivate: [signedIn], title: 'Pengajuan Cuti Saya | CutiApp' },
  { path: 'cuti-details', component: CutiDetailsComponent, canActivate: [signedIn], title: 'Form Pengajuan Cuti | CutiApp' },
  { path: 'approval', component: ApprovalComponent, canActivate: [signedIn], title: 'Approval Pengajuan Cuti | CutiApp' },
  { path: '**', redirectTo: 'login' }
];
