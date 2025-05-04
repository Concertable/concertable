import { AfterViewInit, Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { ListingService } from '../../services/listing/listing.service';
import { Venue } from '../../models/venue';
import { EMPTY, Observable, of } from 'rxjs';
import { Listing } from '../../models/listing';
import { AuthService } from '../../services/auth/auth.service';
import { ListingApplicationService } from '../../services/listing-application/listing-application.service';
import { Genre } from '../../models/genre';
import { GenreService } from '../../services/genre/genre.service';
import { ActivatedRoute, Router } from '@angular/router';
import { ListingToastServiceService } from '../../services/toast/listing/listing-toast-service.service';
import { VenueService } from '../../services/venue/venue.service';

@Component({
  selector: 'app-listings',
  standalone: false,
  templateUrl: './listings.component.html',
  styleUrl: './listings.component.scss'
})
export class ListingsComponent  implements OnInit, OnChanges {
  isLoading: boolean = false;
  currentYear: number = new Date().getFullYear();

  @Input() venueId?: number
  @Input() editMode?: boolean
  @Output() listingCreate = new EventEmitter<Listing>

  listings : Listing[] = [];
  newListing: Listing = {
    startDate: new Date(),
    endDate: new Date(),
    pay: 0,
    genres: []
  };
  genres: Genre[] = [];
  newGenre: Genre = {
    id: 0,
    name: ''
  };
  addNew: boolean = false;
  isOwner$?: Observable<boolean>
  applyResponse$: Observable<void> | null = null;

  constructor(
    private listingService: ListingService, 
    private applicationService: ListingApplicationService, 
    private venueService: VenueService,
    private genreService: GenreService,
    protected authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private listingToastService: ListingToastServiceService) { }

  ngOnInit(): void {
    this.getListings();
    this.getGenres();
    if(this.venueId)
      this.isOwner$ = this.venueService.isOwner(this.venueId);
  }

  isAfterCurrentYear(date: Date): boolean {
    return date.getFullYear() > this.currentYear;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if(changes['venue'])
      this.getListings();
  }

  getGenres() {
    console.log("?")
    this.genreService.getAll().subscribe(g => this.genres = g);
    console.log(this.genres);
  }

  getListings() {
    console.log("getActiveListings");
    if(this.venueId)
      this.listingService.getActiveByVenueId(this.venueId).subscribe((listings) => {this.listings = listings; console.log(this.listings)})
  }

  onAdd() {
    this.addNew = true;
    this.createDummyListing();
  }

  onAddGenre() {
    if (this.newGenre.name) {
      if (!this.newListing.genres.includes(this.newGenre)) {
        this.newListing.genres.push(this.newGenre);
        this.newGenre = { id: 0, name: '' };
      }
    }
  }

  onStartDateChange(date: Date) {
    this.newListing.startDate.setFullYear(date.getFullYear(), date.getMonth(), date.getDate());
    this.newListing.endDate.setFullYear(date.getFullYear(), date.getMonth(), date.getDate());
  }

  onStartTimeChange(time: Date) {
    this.updateTime(this.newListing.startDate, time);
  }

  onEndTimeChange(time: Date) {
    this.updateTime(this.newListing.endDate, time);
  }

  private updateTime(dateRef: Date, time: Date) {
    dateRef.setHours(time.getHours());
    dateRef.setMinutes(time.getMinutes());
    dateRef.setSeconds(0);
    dateRef.setMilliseconds(0);
  }
  
  isEndTimeBeforeStartTime(start: Date, end: Date): boolean {
    const startHours = start.getHours();
    const startMinutes = start.getMinutes();
    const endHours = end.getHours();
    const endMinutes = end.getMinutes();
  
    return (
      endHours < startHours ||
      (endHours === startHours && endMinutes < startMinutes)
    );
  }
  
  

  onSaveItem() {
    // If end time is earlier than start time, assume it's the next day
    if (this.isEndTimeBeforeStartTime(this.newListing.startDate, this.newListing.endDate)) 
      this.newListing.endDate.setDate(this.newListing.startDate.getDate() + 1);
    else 
      this.newListing.endDate.setDate(this.newListing.startDate.getDate());
    
    console.log(this.newListing)
    this.listings?.push(this.newListing);
    this.addNew = false;
    this.listingCreate.emit(this.newListing);
    this.createDummyListing();
  }

  createListing(listing: Listing) {
    this.listingService.create(listing).subscribe();
  }

  onDelete(listing: Listing) {

  }

  onApply(listing: Listing) {
    if(listing.id)
      this.applicationService.applyForListing(listing.id).subscribe(() => {
        this.listingToastService.showApplied('');
    });
  }

  onViewApplications(listing: Listing) {
    console.log(listing);
    this.router.navigate(['venue/my/applications', listing.id]);
  }
  
  private createDummyListing() {
    this.newListing = {
      startDate: new Date(),
      endDate: new Date(),
      pay: 0,
      genres: []
    }
  }
  
}
