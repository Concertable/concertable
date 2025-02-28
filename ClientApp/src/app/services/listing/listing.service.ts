import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, ReplaySubject, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Listing } from '../../models/listing';
import { DateTimeConverterService } from '../date-time-converter/date-time-converter.service';
import { VenueService } from '../venue/venue.service';

@Injectable({
  providedIn: 'root'
})
export class ListingService {

  private apiUrl = `${environment.apiUrl}/listing`;

  private userVenueId?: number;
  private listingIds = new Set<number>(); //prevents duplicates being added to replaysubject

  constructor(private http: HttpClient, private venueService: VenueService) {
    this.venueService.currentUserVenue$.subscribe(venue => {
      this.userVenueId = venue?.id ?? undefined;
    });
   }

  private recentListingsSubject = new ReplaySubject<Listing>(5);
  recentListings$ = this.recentListingsSubject.asObservable();

  create(listing: Listing) : Observable<void> {
    return this.http.post<void>(`${this.apiUrl}`, listing);
  }

  createMultiple(listings: Listing[]) : Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/bulk`, listings);
  }

  getActiveByVenueId(id: number) : Observable<Listing[]> {
    return this.http.get<Listing[]>(`${this.apiUrl}/active/venue/${id}`)
  }

  addToRecentListings() {

  }
}
