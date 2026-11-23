import { Component, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { apiError } from '../../services/api-error';
import { LeaveService } from '../../services/leave.service';

const dateOrder: ValidatorFn = group => {
  const { startDate, endDate } = group.value;
  return startDate && endDate && endDate < startDate ? { dateOrder: true } : null;
};

const reasonLength: ValidatorFn = control => {
  const length = control.value.trim().length;
  return length < 10 ? { shortReason: true } : null;
};

@Component({
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './cuti-details.component.html'
})
export class CutiDetailsComponent {
  private readonly leaves = inject(LeaveService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  readonly form = inject(FormBuilder).nonNullable.group({
    startDate: ['', Validators.required],
    endDate: ['', Validators.required],
    reason: ['', [Validators.required, reasonLength, Validators.maxLength(500)]]
  }, { validators: dateOrder });
  readonly fields = this.form.controls;
  busy = false;
  error = '';

  submit(): void {
    this.form.markAllAsTouched();
    if (this.busy || this.form.invalid) return;
    const { startDate, endDate, reason } = this.form.getRawValue();
    this.busy = true;
    this.error = '';
    this.leaves.create({ startDate, endDate, reason: reason.trim() }).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => void this.router.navigateByUrl('/home'),
      error: err => {
        this.error = apiError(err, 'Pengajuan gagal disimpan. Silakan coba lagi.');
        this.busy = false;
      }
    });
  }
}
