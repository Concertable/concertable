import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { ReviewSummary } from '../../models/review-summary';
import { Observable } from 'rxjs';
import { ReviewParams } from '../../models/review-params';
import { HttpParamsService } from '../http-params/http-params.service';
import { PaginationParams } from '../../models/pagination-params';
import { Pagination } from '../../models/pagination';
import { Review } from '../../models/review';

@Injectable({
  providedIn: 'root'
})
export class ReviewService {
  private apiUrl = `${environment.apiUrl}/review`;

  constructor(private http: HttpClient, private httpParamsService: HttpParamsService<PaginationParams>) { }

  getSummaryByArtistId(id: number): Observable<ReviewSummary> {
    return this.http.get<ReviewSummary>(`${this.apiUrl}/artist/summary/${id}`);
  }

  getSummaryByEventId(id: number): Observable<ReviewSummary> {
    return this.http.get<ReviewSummary>(`${this.apiUrl}/event/summary/${id}`);
  }

  getSummaryByVenueId(id: number): Observable<ReviewSummary> {
    return this.http.get<ReviewSummary>(`${this.apiUrl}/venue/summary/${id}`);
  }

  getByVenueId(id: number, pageParams: PaginationParams): Observable<Pagination<Review>> {
    const params = this.httpParamsService.serialize(pageParams);
    return this.http.get<Pagination<Review>>(`${this.apiUrl}/venue/${id}`, { params });
  }

  getByArtistId(id: number, pageParams: PaginationParams): Observable<Pagination<Review>> {
    const params = this.httpParamsService.serialize(pageParams);
    return this.http.get<Pagination<Review>>(`${this.apiUrl}/artist/${id}`, { params });
  }

  getByEventId(id: number, pageParams: PaginationParams): Observable<Pagination<Review>> {
    const params = this.httpParamsService.serialize(pageParams);
    return this.http.get<Pagination<Review>>(`${this.apiUrl}/event/${id}`, { params });
  }
}
