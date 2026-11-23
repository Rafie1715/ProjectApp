import { Component, Input } from '@angular/core';
import { LeaveStatus } from '../models/leave';

@Component({
  standalone: true,
  selector: 'app-status-badge',
  template: '<span [class]="\'badge \' + status">{{ labels[status] }}</span>'
})
export class StatusBadgeComponent {
  @Input({ required: true }) status!: LeaveStatus;
  readonly labels: Record<LeaveStatus, string> = {
    Pending: 'Menunggu Persetujuan',
    Approved: 'Disetujui',
    Rejected: 'Ditolak'
  };
}
