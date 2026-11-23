export interface CreateLeaveRequest {
  startDate: string;
  endDate: string;
  reason: string;
}

export type LeaveStatus = 'Pending' | 'Approved' | 'Rejected';

export interface LeaveRequest extends CreateLeaveRequest {
  id: number;
  userId: number;
  fullName: string;
  status: LeaveStatus;
  createdAt: string;
}

export interface LeaveBalance {
  remainingDays: number;
}
