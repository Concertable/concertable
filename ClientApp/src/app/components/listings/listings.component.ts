import { AfterViewInit, Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { ListingService } from '../../services/listing/listing.service';
import { Venue } from '../../models/venue';
import { of } from 'rxjs';
import { Listing } from '../../models/listing';
import { AuthService } from '../../services/auth/auth.service';
import { ListingApplicationService } from '../../services/listing-application/listing-application.service';
import { Genre } from '../../models/genre';
import { GenreService } from '../../services/genre/genre.service';
import { ActivatedRoute, Router } from '@angular/router';
import { ListingToastServiceService } from '../../services/toast/listing/listing-toast-service.service';

@Component({
  selector: 'app-listings',
  standalone: false,
  templateUrl: './listings.component.html',
  styleUrl: './listings.component.scss'
})
export class ListingsComponent  implements OnInit, OnChanges {

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


  constructor(
    private listingService: ListingService, 
    private applicationService: ListingApplicationService, 
    private genreService: GenreService,
    protected authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private listingToastService: ListingToastServiceService) { }

  ngOnInit(): void {
    this.getListings();
    this.getGenres();
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
      this.listingService.getActiveByVenueId(this.venueId).subscribe((listings) => this.listings = listings)
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

  set startTime(value: string) {
    if (!this.newListing.startDate || !value) return;

    // Ensure startDate is always a Date object
    if (typeof this.newListing.startDate === 'string') {
        this.newListing.startDate = new Date(this.newListing.startDate + "T00:00:00"); // Convert properly
    }

    const [hours, minutes] = value.split(':').map(Number);
    this.newListing.startDate.setHours(hours, minutes, 0, 0);
}


set endTime(value: string) {
  if (!this.newListing.endDate || !value) return;

  // Ensure endDate is always a Date object
  if (typeof this.newListing.endDate === 'string') {
      this.newListing.endDate = new Date(this.newListing.endDate + "T00:00:00"); // Convert properly
  }

  const [hours, minutes] = value.split(':').map(Number);
  this.newListing.endDate.setHours(hours, minutes, 0, 0);

  // Ensure endDate is always on the same day or the next day
  if (this.newListing.endDate < this.newListing.startDate) {
      this.newListing.endDate.setDate(this.newListing.startDate.getDate() + 1);
  } else {
      this.newListing.endDate.setDate(this.newListing.startDate.getDate());
  }
}

  onSaveItem() {
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
    this.router.navigate(['applications'], {
      relativeTo: this.route,
      queryParams: {listingId: listing.id }
    });
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
