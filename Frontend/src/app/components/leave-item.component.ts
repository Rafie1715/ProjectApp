import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { LeaveRequest } from '../models/leave';
import { StatusBadgeComponent } from './status-badge.component';

@Component({
  standalone: true,
  imports: [DatePipe, StatusBadgeComponent],
  selector: 'app-leave-item',
  template: `
    <article class="card leave-item">
      <div>
        <h2>{{ leave.startDate | date:'dd MMM yyyy' }} — {{ leave.endDate | date:'dd MMM yyyy' }}</h2>
        <p class="reason">{{ leave.reason }}</p>
        <small>Diajukan: {{ leave.createdAt | date:'dd MMM yyyy HH:mm' }}</small>
      </div>
      <div class="item-actions">
        <app-status-badge [status]="leave.status" />
        @if (leave.status === 'Pending') {
          <button type="button" class="btn danger small" [disabled]="disabled"
            (click)="cancel.emit(leave.id)">Batalkan</button>
        }
      </div>
    </article>`
})
export class LeaveItemComponent {
  @Input({ required: true }) leave!: LeaveRequest;
  @Input() disabled = false;
  @Output() readonly cancel = new EventEmitter<number>();
}
