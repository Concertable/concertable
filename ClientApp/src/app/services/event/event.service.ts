import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { Event } from '../../models/event';
import { EventHeader } from '../../models/event-header';
import { Pagination } from '../../models/pagination';
import { ListingApplicationPurchase } from '../../models/listing-application-purchase';
import { DateService } from '../date.service';

@Injectable({
  providedIn: 'root'
})
export class EventService {
  private apiUrl = `${environment.apiUrl}/event`;

  constructor(
    private http: HttpClient,
    private dateService: DateService
  ) {}

  getUpcomingByVenueId(id: number): Observable<Event[]> {
    return this.http.get<Event[]>(`${this.apiUrl}/upcoming/venue/${id}`).pipe(
      tap(events => this.dateService.convertItemsToDates(events))
    );
  }

  getUpcomingByArtistId(id: number): Observable<Event[]> {
    return this.http.get<Event[]>(`${this.apiUrl}/upcoming/artist/${id}`).pipe(
      tap(events => this.dateService.convertItemsToDates(events))
    );
  }

  getHistoryByVenueId(id: number): Observable<Event[]> {
    return this.http.get<Event[]>(`${this.apiUrl}/history/venue/${id}`).pipe(
      tap(events => this.dateService.convertItemsToDates(events))
    );
  }

  getHistoryByArtistId(id: number): Observable<Event[]> {
    return this.http.get<Event[]>(`${this.apiUrl}/history/artist/${id}`).pipe(
      tap(events => this.dateService.convertItemsToDates(events))
    );
  }

  getUnpostedByVenueId(id: number): Observable<Event[]> {
    return this.http.get<Event[]>(`${this.apiUrl}/unposted/venue/${id}`).pipe(
      tap(events => this.dateService.convertItemsToDates(events))
    );
  }

  getUnpostedByArtistId(id: number): Observable<Event[]> {
    return this.http.get<Event[]>(`${this.apiUrl}/unposted/artist/${id}`).pipe(
      tap(events => this.dateService.convertItemsToDates(events))
    );
  }

  getDetailsById(id: number): Observable<Event> {
    return this.http.get<Event>(`${this.apiUrl}/${id}`).pipe(
      tap(event => this.dateService.convertItemToDates(event))
    );
  }

  getDetailsByApplicationId(applicationId: number): Observable<Event> {
    return this.http.get<Event>(`${this.apiUrl}/application/${applicationId}`).pipe(
      tap(event => this.dateService.convertItemToDates(event))
    );
  }

  getHeaders(params: HttpParams): Observable<Pagination<EventHeader>> {
    return this.http.get<Pagination<EventHeader>>(`${this.apiUrl}/headers`, { params });
  }

  getHeadersByAmount(amount: number): Observable<EventHeader[]> {
    return this.http.get<EventHeader[]>(`${this.apiUrl}/headers/amount/${amount}`);
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

  post(event: Event): Observable<Event> {
    return this.http.put<Event>(`${this.apiUrl}/post/${event.id}`, event).pipe(
      tap(event => this.dateService.convertItemToDates(event))
    );
  }

  update(event: Event): Observable<Event> {
    return this.http.put<Event>(`${this.apiUrl}/${event.id}`, event).pipe(
      tap(event => this.dateService.convertItemToDates(event))
    );
  }
}
