import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, ReplaySubject, tap } from 'rxjs';
import { Event } from '../../models/event';
import { EventHeader } from '../../models/event-header';
import { Pagination } from '../../models/pagination';
import { ListingApplicationPurchase } from '../../models/listing-application-purchase';
import { SignalRService } from '../signalr/signalr.service';

@Injectable({
  providedIn: 'root'
})
export class EventService {
  private apiUrl = `${environment.apiUrl}/event`;

  recentEventHeadersSubject = new ReplaySubject<EventHeader>(10);
  recentEventHeaders$ = this.recentEventHeadersSubject.asObservable();

  constructor(
    private http: HttpClient,
    private signalRService: SignalRService
  ) {
    this.loadInitialRecentHeaders();
    this.signalRService.eventPosted$.subscribe(header => {
      if (header) {
        console.log(header);
        this.recentEventHeadersSubject.next(header);
      }
    });
  }

  private loadInitialRecentHeaders(): void {
    this.getLocalHeadersForUser(true, 10).subscribe(headers => {
      headers.forEach(header => this.recentEventHeadersSubject.next(header));
    });
  }

  getUpcomingByVenueId(id: number): Observable<Event[]> {
    return this.http.get<Event[]>(`${this.apiUrl}/upcoming/venue/${id}`);
  }

  getUpcomingByArtistId(id: number): Observable<Event[]> {
    return this.http.get<Event[]>(`${this.apiUrl}/upcoming/artist/${id}`);
  }

  getHistoryByVenueId(id: number): Observable<Event[]> {
    return this.http.get<Event[]>(`${this.apiUrl}/history/venue/${id}`);
  }

  getHistoryByArtistId(id: number): Observable<Event[]> {
    return this.http.get<Event[]>(`${this.apiUrl}/history/artist/${id}`);
  }

  getUnpostedByVenueId(id: number): Observable<Event[]> {
    return this.http.get<Event[]>(`${this.apiUrl}/unposted/venue/${id}`);
  }

  getUnpostedByArtistId(id: number): Observable<Event[]> {
    return this.http.get<Event[]>(`${this.apiUrl}/unposted/artist/${id}`);
  }

  getDetailsById(id: number): Observable<Event> {
    return this.http.get<Event>(`${this.apiUrl}/${id}`);
  }

  getDetailsByApplicationId(applicationId: number): Observable<Event> {
    return this.http.get<Event>(`${this.apiUrl}/application/${applicationId}`);
  }

  getHeaders(params: HttpParams): Observable<Pagination<EventHeader>> {
    return this.http.get<Pagination<EventHeader>>(`${this.apiUrl}/headers`, { params });
  }

  book(paymentMethodId: string, applicationId: number): Observable<ListingApplicationPurchase> {
    return this.http.post<ListingApplicationPurchase>(`${this.apiUrl}/book`, { paymentMethodId, applicationId });
  }

  getLocalHeadersForUser(orderByRecent: boolean = false, take?: number): Observable<EventHeader[]> {
    let params = new HttpParams().set('orderByRecent', orderByRecent);
    if (take != null) {
      params = params.set('take', take.toString());
    }

    return this.http.get<EventHeader[]>(`${this.apiUrl}/headers/local/user`, { params });
  }

  addFakeEvent(header: EventHeader): void {
    this.recentEventHeadersSubject.next(header);
  }
  
}
