import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, ReplaySubject } from 'rxjs';
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

  private recentEventsSubject = new ReplaySubject<Event>(5);
  recentEvents$ = this.recentEventsSubject.asObservable();
  
  constructor(private http: HttpClient, private signalRService: SignalRService) {
    this.signalRService.eventPosted$.subscribe(event => {
      console.log("POSTEDDDD");
      if(event)
        this.recentEventsSubject.next(event);
    })
  }

  getUpComingByVenueId(id: number) : Observable<Event[]> {
    return this.http.get<Event[]>(`${this.apiUrl}/upcoming/venue/${id}`);
  }

  getUpComingByArtistId(id: number) : Observable<Event[]> {
    return this.http.get<Event[]>(`${this.apiUrl}/upcoming/artist/${id}`);
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

  book(paymentMethodId: string, applicationId: number) : Observable<ListingApplicationPurchase> {
    return this.http.post<ListingApplicationPurchase>(`${this.apiUrl}/book`, { paymentMethodId, applicationId });
  }
}
