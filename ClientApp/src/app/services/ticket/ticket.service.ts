import { HttpClient, HttpContext } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Ticket } from '../../models/ticket';
import { environment } from '../../../environments/environment';
import { TicketPurchase } from '../../models/ticket-purchase';
import { SKIP_ERROR_HANDLER } from '../../shared/http/http-context.token';

@Injectable({
  providedIn: 'root',
})
export class TicketService {
  private apiUrl = `${environment.apiUrl}/ticket`;

  constructor(private http: HttpClient) {}

  purchase(paymentMethodId: string, eventId: number, quantity: number): Observable<TicketPurchase> {
    return this.http.post<TicketPurchase>(`${this.apiUrl}/purchase`, {
      eventId,
      paymentMethodId,
    });
  }

  getUserHistory(): Observable<Ticket[]> {
    return this.http.get<Ticket[]>(`${this.apiUrl}/history/user`);
  }

  getUserUpcoming(): Observable<Ticket[]> {
    return this.http.get<Ticket[]>(`${this.apiUrl}/upcoming/user`);
  }

  canPurchase(eventId: number): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/can-purchase/${eventId}`, {
      context: new HttpContext().set(SKIP_ERROR_HANDLER, true)
    });
  }
}
