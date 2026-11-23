import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { LoginRequest, LoginResponse } from '../models/auth';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  readonly loginUrl = environment.apiUrl + '/auth/login';

  get token(): string | null {
    return localStorage.getItem('cutiapp.token');
  }

  get role(): string | null {
    return localStorage.getItem('cutiapp.role');
  }

  get fullName(): string | null {
    return localStorage.getItem('cutiapp.fullName');
  }

  get landingPage(): string {
    return this.role === 'Manager' ? '/approval' : '/home';
  }

  login(body: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(this.loginUrl, body).pipe(
      tap(session => {
        localStorage.setItem('cutiapp.fullName', session.fullName);
        localStorage.setItem('cutiapp.role', session.role);
        localStorage.setItem('cutiapp.token', session.token);
      })
    );
  }

  logout(): void {
    localStorage.removeItem('cutiapp.token');
    localStorage.removeItem('cutiapp.role');
    localStorage.removeItem('cutiapp.fullName');
  }
}
