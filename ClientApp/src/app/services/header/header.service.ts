import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { VenueHeader } from '../../models/venue-header';
import { forkJoin, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ArtistHeader } from '../../models/artist-header';
import { EventHeader } from '../../models/event-header';
import { SearchParams } from '../../models/search-params';
import { VenueService } from '../venue/venue.service';
import { ArtistService } from '../artist/artist.service';
import { EventService } from '../event/event.service';
import { Pagination } from '../../models/pagination';
import { Venue } from '../../models/venue';
import { Params } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class HeaderService {

  apiUrl = `${environment.apiUrl}/header`

  constructor(
    private http: HttpClient,
    private venueService: VenueService,
    private artistService: ArtistService,
    private eventService: EventService
  ) { }

  // getVenueHeaders(searchParams: SearchParams): Observable<Pagination<VenueHeader>> {
  //   const params = this.setParams(searchParams);
  //   return this.venueService.getHeaders(params);
  // }

  // getArtistHeaders(searchParams: SearchParams): Observable<Pagination<ArtistHeader>> {
  //   const params = this.setParams(searchParams);
  //   return this.artistService.getHeaders(params);
  // }

  // getEventHeaders(searchParams: SearchParams): Observable<Pagination<EventHeader>> {
  //   const params = this.setParams(searchParams);
  //   console.log("xxx",searchParams);
  //   console.log(params);
  //   return this.eventService.getHeaders(params);
  // }

  get<T extends ArtistHeader | VenueHeader | EventHeader>(params: Params): Observable<Pagination<T>> {
    const httpParams = new HttpParams({ fromObject: params })
    return this.http.get<Pagination<T>>(`${this.apiUrl}`, { params })
  }
}
