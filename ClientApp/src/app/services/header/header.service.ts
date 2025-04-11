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
import { SearchParamsSerializerServiceService } from '../search-params-serializer/search-params-serializer-service.service';

@Injectable({
  providedIn: 'root'
})
export class HeaderService {

  apiUrl = `${environment.apiUrl}/header`

  constructor(
    private http: HttpClient,
    private serializerService: SearchParamsSerializerServiceService
  ) { }

  get<T extends ArtistHeader | VenueHeader | EventHeader>(searchParams: Partial<SearchParams>): Observable<Pagination<T>> {
    const params = this.serializerService.serialize(searchParams);
    return this.http.get<Pagination<T>>(`${this.apiUrl}`, { params })
  }
}
