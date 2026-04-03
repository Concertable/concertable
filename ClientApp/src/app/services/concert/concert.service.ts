import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { Concert } from '../../models/concert';
import { ConcertHeader } from '../../models/concert-header';
import { Pagination } from '../../models/pagination';
import { ListingApplicationPurchase } from '../../models/listing-application-purchase';
import { DateService } from '../date.service';

@Injectable({
  providedIn: 'root'
})
export class ConcertService {
  private apiUrl = `${environment.apiUrl}/concert`;

  constructor(
    private http: HttpClient,
    private dateService: DateService
  ) {}

  getUpcomingByVenueId(id: number): Observable<Concert[]> {
    return this.http.get<Concert[]>(`${this.apiUrl}/upcoming/venue/${id}`).pipe(
      tap(concerts => this.dateService.convertItemsToDates(concerts))
    );
  }

  getUpcomingByArtistId(id: number): Observable<Concert[]> {
    return this.http.get<Concert[]>(`${this.apiUrl}/upcoming/artist/${id}`).pipe(
      tap(concerts => this.dateService.convertItemsToDates(concerts))
    );
  }

  getHistoryByVenueId(id: number): Observable<Concert[]> {
    return this.http.get<Concert[]>(`${this.apiUrl}/history/venue/${id}`).pipe(
      tap(concerts => this.dateService.convertItemsToDates(concerts))
    );
  }

  getHistoryByArtistId(id: number): Observable<Concert[]> {
    return this.http.get<Concert[]>(`${this.apiUrl}/history/artist/${id}`).pipe(
      tap(concerts => this.dateService.convertItemsToDates(concerts))
    );
  }

  getUnpostedByVenueId(id: number): Observable<Concert[]> {
    return this.http.get<Concert[]>(`${this.apiUrl}/unposted/venue/${id}`).pipe(
      tap(concerts => this.dateService.convertItemsToDates(concerts))
    );
  }

  getUnpostedByArtistId(id: number): Observable<Concert[]> {
    return this.http.get<Concert[]>(`${this.apiUrl}/unposted/artist/${id}`).pipe(
      tap(concerts => this.dateService.convertItemsToDates(concerts))
    );
  }

  getDetailsById(id: number): Observable<Concert> {
    return this.http.get<Concert>(`${this.apiUrl}/${id}`).pipe(
      tap(concert => this.dateService.convertItemToDates(concert))
    );
  }

  getDetailsByApplicationId(applicationId: number): Observable<Concert> {
    return this.http.get<Concert>(`${this.apiUrl}/application/${applicationId}`).pipe(
      tap(concert => this.dateService.convertItemToDates(concert))
    );
  }

  getHeaders(params: HttpParams): Observable<Pagination<ConcertHeader>> {
    return this.http.get<Pagination<ConcertHeader>>(`${this.apiUrl}/headers`, { params });
  }

  getHeadersByAmount(amount: number): Observable<ConcertHeader[]> {
    return this.http.get<ConcertHeader[]>(`${this.apiUrl}/headers/amount/${amount}`);
  }

  book(paymentMethodId: string, applicationId: number): Observable<ListingApplicationPurchase> {
    return this.http.post<ListingApplicationPurchase>(`${this.apiUrl}/book`, { paymentMethodId, applicationId });
  }

  getRecommendedHeaders(): Observable<ConcertHeader[]> {
    return this.http.get<ConcertHeader[]>(`${this.apiUrl}/headers/recommended`);
  }

  getPopularHeaders(): Observable<ConcertHeader[]> {
    return this.http.get<ConcertHeader[]>(`${this.apiUrl}/headers/popular`);
  }

  getFreeHeaders(): Observable<ConcertHeader[]> {
    return this.http.get<ConcertHeader[]>(`${this.apiUrl}/headers/free`);
  }

  post(concert: Concert): Observable<Concert> {
    return this.http.put<Concert>(`${this.apiUrl}/post/${concert.id}`, concert).pipe(
      tap(concert => this.dateService.convertItemToDates(concert))
    );
  }

  update(concert: Concert): Observable<Concert> {
    return this.http.put<Concert>(`${this.apiUrl}/${concert.id}`, concert).pipe(
      tap(concert => this.dateService.convertItemToDates(concert))
    );
  }
}
