import { Component, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { apiError } from '../../services/api-error';

@Component({
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  readonly form = inject(FormBuilder).nonNullable.group({
    username: ['', [Validators.required, Validators.pattern(/\S/)]],
    password: ['', Validators.required]
  });
  error = inject(ActivatedRoute).snapshot.queryParamMap.has('expired')
    ? 'Sesi Anda telah berakhir. Silakan login kembali.' : '';
  busy = false;

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.busy) return;
    this.error = '';
    this.busy = true;
    const { username, password } = this.form.getRawValue();
    this.auth.login({ username: username.trim(), password }).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => void this.router.navigateByUrl(this.auth.landingPage),
      error: err => {
        this.busy = false;
        this.error = apiError(err, err.status === 401
          ? 'Username atau password salah.' : 'Login gagal. Silakan coba lagi.');
      }
    });
  }
}
