import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ListingApplicationService {

  private apiUrl = `${environment.apiUrl}/listingapplication`;
  
  constructor(private http: HttpClient) { }

  applyForListingAsync(id: number) : Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}`, {});
  }
}
