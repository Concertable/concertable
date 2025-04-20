import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, ReplaySubject, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Listing } from '../../models/listing';
import { DateService } from '../date.service'; // Import the DateService
import { VenueService } from '../venue/venue.service';

@Injectable({
  providedIn: 'root'
})
export class ListingService {

  private apiUrl = `${environment.apiUrl}/listing`;

  private userVenueId?: number;
  private listingIds = new Set<number>(); // Prevents duplicates being added to ReplaySubject

  constructor(
    private http: HttpClient,
    private venueService: VenueService,
    private dateService: DateService // Inject DateService
  ) {
    this.venueService.currentUserVenue$.subscribe(venue => {
      this.userVenueId = venue?.id ?? undefined;
    });
  }

  private recentListingsSubject = new ReplaySubject<Listing>(5);
  recentListings$ = this.recentListingsSubject.asObservable();

  create(listing: Listing): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}`, listing);
  }

  getActiveByVenueId(id: number): Observable<Listing[]> {
    return this.http.get<Listing[]>(`${this.apiUrl}/active/venue/${id}`).pipe(
      tap((listings) => {
        // Use DateService to convert startDate and endDate to Date objects for each listing
        this.dateService.convertItemsToDates(listings); // Use DateService for date conversion
      })
    );
  }

  addToRecentListings() {
    // This method can be implemented based on your logic (currently empty)
  }

  isOwner(id: number): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/is-owner/${id}`);
  }
}
