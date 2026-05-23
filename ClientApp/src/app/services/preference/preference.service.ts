import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { Preference } from '../../models/preference';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class PreferenceService {
private apiUrl = `${environment.apiUrl}/preference`;

  constructor(private http: HttpClient) { }

  getByUser(): Observable<Preference> {
    return this.http.get<Preference>(`${this.apiUrl}/user`);
  }
  
  update(preference: Preference): Observable<Preference> {
    return this.http.put<Preference>(`${this.apiUrl}/${preference.id}`, preference);
  }

  create(preference: Preference): Observable<Preference> {
    return this.http.post<Preference>(`${this.apiUrl}`, preference);
  }
}
