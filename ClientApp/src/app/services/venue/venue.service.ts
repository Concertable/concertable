import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Venue } from '../../models/venue';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class VenueService {

  constructor(private http: HttpClient) { }

  private apiUrl = `${environment.apiUrl}/venue`

  getVenueDetailsById(id: number) {
    return this.http.get<Venue>(`${this.apiUrl}/${id}`);
  }

  getVenueHeaders() : Observable<Venue[]> {
    let params = new HttpParams();
    return this.http.get<Venue[]>(`${this.apiUrl}/headers`, { params }); 
  }

  getUserVenue() : Observable<Venue> {
    return this.http.get<Venue>(`${this.apiUrl}/user-venue`);
  }
}
