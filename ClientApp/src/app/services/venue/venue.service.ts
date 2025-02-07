import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Venue } from '../../models/venue';
import { environment } from '../../../environments/environment';
import { VenueHeader } from '../../models/venue-header';

@Injectable({
  providedIn: 'root'
})
export class VenueService {

  constructor(private http: HttpClient) { }

  private apiUrl = `${environment.apiUrl}/venue`

  getDetailsById(id: number) {
    return this.http.get<Venue>(`${this.apiUrl}/${id}`);
  }

  getHeaders(params: HttpParams) : Observable<VenueHeader[]> {
    return this.http.get<VenueHeader[]>(`${this.apiUrl}/headers`, { params }); 
  }

  getUserVenue() : Observable<Venue> {
    return this.http.get<Venue>(`${this.apiUrl}/user-venue`);
  }
}
