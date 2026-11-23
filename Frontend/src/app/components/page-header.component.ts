import { Component, Input, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  standalone: true,
  selector: 'app-page-header',
  template: `
    <header class="page-header">
      <div class="container header-inner">
        <h1>CutiApp — {{ title }}</h1>
        <div class="account"><span>{{ auth.fullName }}</span>
          <button type="button" class="link-button" (click)="logout()">Logout</button>
        </div>
      </div>
    </header>`
})
export class PageHeaderComponent {
  @Input({ required: true }) title!: string;
  protected readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  logout(): void {
    this.auth.logout();
    void this.router.navigateByUrl('/login');
  }
}
