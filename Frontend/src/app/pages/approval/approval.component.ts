import { DatePipe, TitleCasePipe } from '@angular/common';
import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { PageHeaderComponent } from '../../components/page-header.component';
import { StatusBadgeComponent } from '../../components/status-badge.component';
import { LeaveRequest } from '../../models/leave';
import { apiError } from '../../services/api-error';
import { LeaveService } from '../../services/leave.service';

@Component({
  standalone: true,
  imports: [DatePipe, TitleCasePipe, PageHeaderComponent, StatusBadgeComponent],
  templateUrl: './approval.component.html'
})
export class ApprovalComponent implements OnInit {
  private readonly service = inject(LeaveService);
  private readonly destroyRef = inject(DestroyRef);
  leaves: LeaveRequest[] = [];
  loading = false;
  processing = false;
  actionError = '';
  listError = '';

  ngOnInit(): void {
    this.loadList();
  }

  loadList(): void {
    this.loading = true;
    this.listError = '';
    this.service.all().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: leaves => {
        this.leaves = leaves;
        this.loading = false;
      },
      error: err => {
        this.loading = false;
        this.listError = apiError(err, 'Daftar pengajuan gagal dimuat.');
      }
    });
  }

  decide(id: number, action: 'approve' | 'reject'): void {
    if (this.processing || this.loading) return;
    this.actionError = '';
    this.processing = true;
    const decision = action === 'approve' ? this.service.approve(id) : this.service.reject(id);
    decision.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.processing = false;
        this.loadList();
      },
      error: err => {
        this.processing = false;
        this.actionError = apiError(err, 'Pengajuan gagal diproses.');
        if (err.status === 409 || err.status === 404) this.loadList();
      }
    });
  }
}
