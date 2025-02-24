import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { TicketPurchase } from '../../models/ticket-purchase';

@Injectable({
  providedIn: 'root'
})
export class TicketService {
  private apiUrl = `${environment.apiUrl}/ticket`;

  constructor(private http: HttpClient) {}

  purchase(paymentMethodId: string, eventId: number): Observable<TicketPurchase> {
    return this.http.post<TicketPurchase>(`${this.apiUrl}/purchase`, { eventId, paymentMethodId } );
  }
}
