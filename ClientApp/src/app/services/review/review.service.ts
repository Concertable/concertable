import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { ReviewSummary } from '../../models/review-summary';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ReviewService {
  private apiUrl = `${environment.apiUrl}/review`;

  constructor(private http: HttpClient) { }

  getSummaryByArtistId(id: number): Observable<ReviewSummary> {
    return this.http.get<ReviewSummary>(`${this.apiUrl}/artist/${id}/summary`);
  }

  getSummaryByEventId(id: number): Observable<ReviewSummary> {
    return this.http.get<ReviewSummary>(`${this.apiUrl}/event/${id}/summary`);
  }

  getSummaryByVenueId(id: number): Observable<ReviewSummary> {
    return this.http.get<ReviewSummary>(`${this.apiUrl}/venue/${id}/summary`);
  }
}
