import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Genre } from '../../models/genre';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class GenreService {

  private apiUrl = `${environment.apiUrl}/listing`;

  constructor(private http: HttpClient) { }

  getAll(): Observable<Genre[]> {
    return this.http.get<Genre[]>(`${this.apiUrl}/all`)
  }
}
