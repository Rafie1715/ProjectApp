import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { LeaveItemComponent } from '../../components/leave-item.component';
import { PageHeaderComponent } from '../../components/page-header.component';
import { LeaveRequest } from '../../models/leave';
import { apiError } from '../../services/api-error';
import { LeaveService } from '../../services/leave.service';

@Component({
  standalone: true,
  imports: [LeaveItemComponent, PageHeaderComponent, RouterLink],
  templateUrl: './home.component.html'
})
export class HomeComponent implements OnInit {
  private readonly service = inject(LeaveService);
  private readonly destroyRef = inject(DestroyRef);
  leaves: LeaveRequest[] = [];
  remainingDays: number | null = null;
  loading = false;
  cancelling = false;
  balanceError = '';
  listError = '';
  actionError = '';

  ngOnInit(): void {
    this.loadList();
    this.loadBalance();
  }

  loadBalance(): void {
    this.balanceError = '';
    this.service.balance().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: balance => this.remainingDays = balance.remainingDays,
      error: err => this.balanceError = apiError(err, 'Saldo cuti gagal dimuat.')
    });
  }

  loadList(): void {
    this.loading = true;
    this.listError = '';
    this.service.mine().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: leaves => {
        this.leaves = leaves;
        this.loading = false;
      },
      error: err => {
        this.listError = apiError(err, 'Daftar pengajuan gagal dimuat.');
        this.loading = false;
      }
    });
  }

  cancel(id: number): void {
    if (this.cancelling || this.loading) return;
    this.cancelling = true;
    this.actionError = '';
    this.service.cancel(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.cancelling = false;
        this.loadList();
      },
      error: err => {
        this.actionError = apiError(err, 'Pengajuan gagal dibatalkan.');
        this.cancelling = false;
        if (err.status === 404 || err.status === 409) this.loadList();
      }
    });
  }
}
