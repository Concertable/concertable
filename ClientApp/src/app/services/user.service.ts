import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { User } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  private apiUrl = `${environment.apiUrl}/users`;

  constructor(private http: HttpClient) {}

  updateLocation(userId: number, latitude: number, longitude: number): Observable<User> {
    return this.http.put<User>(`${this.apiUrl}/${userId}/location`, { latitude, longitude });
  }
}
