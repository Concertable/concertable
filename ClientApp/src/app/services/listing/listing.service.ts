import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Listing } from '../../models/listing';

@Injectable({
  providedIn: 'root'
})
export class ListingService {

  private apiUrl = `${environment.apiUrl}/listing`;

  constructor(private http: HttpClient) { }

  create(listing: Listing) : Observable<void> {
    return this.http.post<void>(`${this.apiUrl}`, listing);
  }

  getActiveByVenueId(id: number) : Observable<Listing[]> {
    return this.http.get<Listing[]>(`${this.apiUrl}/active/venue/${id}`);
  }
}
