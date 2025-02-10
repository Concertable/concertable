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

  getVenueHeaders(searchParams: SearchParams): Observable<Pagination<VenueHeader>> {
    const { date, ...filteredParams } = searchParams; 
    const params = new HttpParams({ fromObject: filteredParams as any })
    return this.venueService.getHeaders(params);
  }

  getArtistHeaders(searchParams: SearchParams): Observable<Pagination<ArtistHeader>> {
    const { date, ...filteredParams } = searchParams; 
    const params = new HttpParams({ fromObject: filteredParams as any })
    return this.artistService.getHeaders(params);
  }

  getEventHeaders(searchParams: SearchParams): Observable<Pagination<EventHeader>> {
    const params = new HttpParams({ fromObject: searchParams as any })
    return this.eventService.getHeaders(params);
  }
}
