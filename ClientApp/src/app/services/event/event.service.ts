import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, ReplaySubject, tap } from 'rxjs';
import { Event } from '../../models/event';
import { EventHeader } from '../../models/event-header';
import { Pagination } from '../../models/pagination';
import { ListingApplicationPurchase } from '../../models/listing-application-purchase';
import { SignalRService } from '../signalr/signalr.service';
import { EventToastService } from '../toast/event/event-toast.service';

@Injectable({
  providedIn: 'root'
})
export class EventService {
  private apiUrl = `${environment.apiUrl}/event`;

  recentEventHeadersSubject = new ReplaySubject<EventHeader>(10);
  recentEventHeaders$ = this.recentEventHeadersSubject.asObservable();

  constructor(
    private http: HttpClient,
    private signalRService: SignalRService,
    private eventToastService: EventToastService
  ) {
    this.loadInitialRecommendedHeaders();
    this.signalRService.eventPosted$.subscribe(header => {
      console.log(header)
      if (header) {
        this.eventToastService.showRecommended(header.id);
        this.recentEventHeadersSubject.next(header);
      }
    });
  }

  private loadInitialRecommendedHeaders(): void {
    this.getRecommendedHeaders().subscribe(headers => {
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

  getHeadersByAmount(amount: number): Observable<EventHeader[]> {
    return this.http.get<EventHeader[]>(`${this.apiUrl}/headers/amount/${amount}`,);
  }

  book(paymentMethodId: string, applicationId: number): Observable<ListingApplicationPurchase> {
    return this.http.post<ListingApplicationPurchase>(`${this.apiUrl}/book`, { paymentMethodId, applicationId });
  }

  getRecommendedHeaders(): Observable<EventHeader[]> {
    return this.http.get<EventHeader[]>(`${this.apiUrl}/headers/recommended`);
  }

  getPopularHeaders(): Observable<EventHeader[]> {
    return this.http.get<EventHeader[]>(`${this.apiUrl}/headers/popular`);
  }

  getFreeHeaders(): Observable<EventHeader[]> {
    return this.http.get<EventHeader[]>(`${this.apiUrl}/headers/free`);
  }

  addFakeEvent(header: EventHeader): void {
    this.recentEventHeadersSubject.next(header);
  }

  post(event: Event): Observable<Event> {
    return this.http.put<Event>(`${this.apiUrl}/post/${event.id}`, event);
  }

  update(event: Event): Observable<Event> {
    return this.http.put<Event>(`${this.apiUrl}/${event.id}`, event);
  }
  
}
