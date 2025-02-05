import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { VenueHeader } from '../../models/venue-header';
import { forkJoin, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ArtistHeader } from '../../models/artist-header';
import { EventHeader } from '../../models/event-header';
import { SearchParams } from '../../models/search-params';

@Injectable({
  providedIn: 'root'
})
export class HeaderService {

  apiUrl = `${environment.apiUrl}/header`

  constructor(private http: HttpClient) { }

  private getParams(searchParams: SearchParams): HttpParams {
    return new HttpParams({ fromObject: searchParams as any });
  }

  getVenueHeaders(searchParams: SearchParams): Observable<VenueHeader[]> {
    const params = this.getParams(searchParams);
    return this.http.get<VenueHeader[]>(`${this.apiUrl}/venue`);
  }

  getArtistHeaders(searchParams: SearchParams): Observable<ArtistHeader[]> {
    const params = this.getParams(searchParams);
    return this.http.get<ArtistHeader[]>(`${this.apiUrl}/venue`);
  }

  getEventHeaders(searchParams: SearchParams): Observable<EventHeader[]> {
    const params = this.getParams(searchParams);
    return this.http.get<EventHeader[]>(`${this.apiUrl}/venue`);
  }
}
