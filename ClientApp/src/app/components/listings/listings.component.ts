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

  get startDate(): string {
    return this.newListing.startDate ? this.newListing.startDate.toISOString().split('T')[0] : '';
  }

  set startDate(value: string) {
    if (value) {
      const time = this.newListing.startDate ? this.newListing.startDate.toTimeString().split(' ')[0] : '00:00:00';
      this.newListing.startDate = new Date(`${value}T${time}`);
    }
  }

  get startTime(): string {
    return this.newListing.startDate ? this.newListing.startDate.toTimeString().slice(0, 5) : '';
  }

  set startTime(value: string) {
    if (value) {
      const date = this.startDate;
      this.newListing.startDate = new Date(`${date}T${value}`);
    }
  }

  get endTime(): string {
    return this.newListing.endDate ? this.newListing.endDate.toTimeString().slice(0, 5) : '';
  }

  set endTime(value: string) {
    if (value) {
      const currentDate = this.newListing.endDate ? this.newListing.endDate.toISOString().split('T')[0] : new Date().toISOString().split('T')[0];
      this.newListing.endDate = new Date(`${currentDate}T${value}`);
    }
  }

  onStartDateChange(date: Date) {
    this.newListing.startDate.setFullYear(date.getFullYear());
    this.newListing.startDate.setMonth(date.getMonth());
    this.newListing.startDate.setDate(date.getDate());
  }

  onStartTimeChange(time: Date) {
    this.newListing.startDate.setHours(time.getHours());
    this.newListing.startDate.setMinutes(time.getMinutes());
    this.newListing.startDate.setSeconds(0);
    this.newListing.startDate.setMilliseconds(0);
  }

  onEndTimeChange(time: Date) {
    this.newListing.endDate.setHours(time.getHours());
    this.newListing.endDate.setMinutes(time.getMinutes());
    this.newListing.endDate.setSeconds(0);
    this.newListing.endDate.setMilliseconds(0);
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
    this.router.navigate(['my/applications', listing.id], {
      relativeTo: this.route.parent
    });
  }
  

  //?
  private createDummyListing() {
    this.newListing = {
      startDate: new Date(),
      endDate: new Date(),
      pay: 0,
      genres: []
    }
  }
  
}
