import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Artist } from '../../models/artist';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ArtistHeader } from '../../models/artist-header';

@Injectable({
  providedIn: 'root'
})
export class ArtistService {
  private apiUrl = `${environment.apiUrl}/artist`;

  constructor(private http: HttpClient) { }

  getDetailsById(id: number): Observable<Artist> {
    return this.http.get<Artist>(`${this.apiUrl}/${id}`);
  }

  getHeaders(params: HttpParams): Observable<ArtistHeader[]> {
    return this.http.get<ArtistHeader[]>(`${this.apiUrl}/headers`);
  }
}
