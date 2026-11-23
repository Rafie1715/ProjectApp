import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { CreateLeaveRequest, LeaveBalance, LeaveRequest } from '../models/leave';

@Injectable({ providedIn: 'root' })
export class LeaveService {
  private readonly http = inject(HttpClient);
  private readonly url = environment.apiUrl + '/leave-requests';

  all(): Observable<LeaveRequest[]> {
    return this.http.get<LeaveRequest[]>(this.url);
  }

  mine(): Observable<LeaveRequest[]> {
    return this.http.get<LeaveRequest[]>(this.url + '/mine');
  }

  balance(): Observable<LeaveBalance> {
    return this.http.get<LeaveBalance>(this.url + '/balance');
  }

  create(body: CreateLeaveRequest): Observable<LeaveRequest> {
    return this.http.post<LeaveRequest>(this.url, body);
  }

  approve(id: number): Observable<LeaveRequest> {
    return this.http.put<LeaveRequest>(this.url + '/' + id + '/approve', null);
  }

  reject(id: number): Observable<LeaveRequest> {
    return this.http.put<LeaveRequest>(this.url + '/' + id + '/reject', null);
  }

  cancel(id: number): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(this.url + '/' + id);
  }
}
