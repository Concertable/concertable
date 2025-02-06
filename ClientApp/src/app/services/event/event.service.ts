import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Event } from '../../models/event';

@Injectable({
  providedIn: 'root'
})
export class EventService {

  private apiUrl = `${environment.apiUrl}/event`;
  
  constructor(private http: HttpClient) { }

  getUpComingEventsByVenueId(id: number) : Observable<Event[]> {
    return this.http.get<Event[]>(`${this.apiUrl}/venue/${id}`);
  }

  getDetailsById(id: number): Observable<Event> {
    return this.http.get<Event>(`${this.apiUrl}/${id}`);
  }
}
