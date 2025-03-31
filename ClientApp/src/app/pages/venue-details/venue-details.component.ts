import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Venue } from '../../models/venue';
import { VenueService } from '../../services/venue/venue.service';
import { NavItem } from '../../models/nav-item';
import { AuthService } from '../../services/auth/auth.service';
import { ActivatedRoute, Router } from '@angular/router';
import { DetailsDirective } from '../../directives/details/details.directive';
import { Observable } from 'rxjs';
import { Coordinates } from '../../models/coordinates';
import { cloneDeep } from 'lodash';
import { Listing } from '../../models/listing';
import { BlobStorageService } from '../../services/blob-storage/blob-storage.service';
import { ExtendedDetailsDirective } from '../../directives/extended-details/extended-details.directive';
import { ToastService } from '../../services/toast/toast.service';
import { GenreService } from '../../services/genre/genre.service';

@Component({
  selector: 'app-venue-details',
  standalone: false,
  
  templateUrl: './venue-details.component.html',
  styleUrl: './venue-details.component.scss'
})
export class VenueDetailsComponent extends ExtendedDetailsDirective<Venue> {
  @Output() listingCreate = new EventEmitter<Listing>

  override navItems: NavItem[] = [
    { name: 'Info', fragment: 'info' },
    { name: 'Location', fragment: 'location' },
    { name: 'Events', fragment: 'events' },
    { name: 'Videos', fragment: 'videos' },
    { name: 'Reviews', fragment: 'reviews' }
  ];

  constructor(
    private venueService: VenueService, 
    authService: AuthService, 
    route: ActivatedRoute, 
    blobStorageService: BlobStorageService,
    genreService: GenreService,
    router: Router,
    toastService: ToastService
  ) {
    super(blobStorageService, genreService, authService, route, router, toastService);
  }

  get venue(): Venue | undefined {
    return this.entity;
  }

  @Input()
  set venue(value: Venue | undefined) {
    this.entity = value;
  }

  override ngOnInit() {
    if(this.authService.isNotRole('Customer')) {
      this.navItems.push({ name: 'Listings', fragment: 'listings' })
    }
    super.ngOnInit();
  }

  setDetails(data: any): void {
    this.venue = data['venue'];
  }

  updateCoordinates(coordinates: Coordinates | undefined) {
    if(this.venue)
    {
      if(coordinates) {
        this.venue.latitude = coordinates.lat;
        this.venue.longitude = coordinates.lng;
      }
    }
    this.onChangeDetected();
  }

  onListingCreate(listing: Listing) {
    this.listingCreate.emit(listing);
  }

  updateImage(url: string) {
    if(this.venue)
      this.venue.imageUrl = url
    this.onChangeDetected
  }

}
