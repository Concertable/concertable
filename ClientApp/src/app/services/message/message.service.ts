import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Message } from '../../models/message';
import { Pagination } from '../../models/pagination';
import { MessageSummary } from '../../models/message-summary';
import { PaginationParams } from '../../models/pagination-params';
import { HttpParamsService } from '../http-params/http-params.service';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  private apiUrl = `${environment.apiUrl}/message`;
    
  constructor(private http: HttpClient, private httpParamsService: HttpParamsService<PaginationParams>) { }

  getForUser(pageParams: PaginationParams): Observable<Pagination<Message>> {
    const params = this.httpParamsService.serialize(pageParams);
    return this.http.get<Pagination<Message>>(`${this.apiUrl}/user`, { params });
  }  

  getSummaryForUser(): Observable<MessageSummary> {
    return this.http.get<MessageSummary>(`${this.apiUrl}/user/summary`);
  }

  getUnreadCountForUser(): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/user/unread-count`);
  }  

  markAsRead(ids: number[]): Observable<number> {
    return this.http.post<number>(`${this.apiUrl}/mark-read`, { messageIds: ids })
  }
}
