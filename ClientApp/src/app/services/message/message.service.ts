import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Message } from '../../models/message';
import { Pagination } from '../../models/pagination';
import { MessageSummary } from '../../models/message-summary';

@Injectable({
  providedIn: 'root'
})
export class MessageService {

  private apiUrl = `${environment.apiUrl}/message`;
    
  constructor(private http: HttpClient) { }

  getAllForUser(): Observable<Pagination<Message>> {
    return this.http.get<Pagination<Message>>(`${this.apiUrl}/all`);
  }

  getSummaryForUser(): Observable<MessageSummary> {
    return this.http.get<MessageSummary>(`${this.apiUrl}/summary`);
  }
}
