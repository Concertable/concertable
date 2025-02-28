import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Artist } from '../../models/artist';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ArtistHeader } from '../../models/artist-header';
import { Pagination } from '../../models/pagination';

@Injectable({
  providedIn: 'root'
})
export class ArtistService {
  private apiUrl = `${environment.apiUrl}/artist`;

  constructor(private http: HttpClient) { }

  private currentUserArtistSubject = new BehaviorSubject<Artist | undefined>(undefined);

  getDetailsById(id: number): Observable<Artist> {
    return this.http.get<Artist>(`${this.apiUrl}/${id}`);
  }

  getHeaders(params: HttpParams): Observable<Pagination<ArtistHeader>> {
    return this.http.get<Pagination<ArtistHeader>>(`${this.apiUrl}/headers`, { params });
  }

  getDetailsForCurrentUser(): Observable<Artist> {
    return this.http.get<Artist>(`${this.apiUrl}/user`).pipe(
      tap(artist => this.currentUserArtistSubject.next(artist))
    );
  }

  create(artist: Artist): Observable<Artist> {
    return this.http.post<Artist>(`${this.apiUrl}`, artist);
  }

  update(artist: Artist) : Observable<Artist> {
    return this.http.put<Artist>(`${this.apiUrl}/${artist.id}`, artist)
  }
}
