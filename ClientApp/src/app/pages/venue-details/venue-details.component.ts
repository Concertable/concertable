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
import { EventViewType } from '../../models/event-view-type';
import { Genre } from '../../models/genre';

@Component({
  selector: 'app-venue-details',
  standalone: false,
  
  templateUrl: './venue-details.component.html',
  styleUrl: './venue-details.component.scss'
})
export class VenueDetailsComponent extends ExtendedDetailsDirective<Venue> {
  @Output() listingCreate = new EventEmitter<Listing>
  @Output() imageChange = new EventEmitter<File>
  image?: File;

  venueGenres: Genre[] = [
    { id: 1, name: 'Rock' },
    { id: 2, name: 'Hip-Hop' },
    { id: 3, name: 'Reggae' },
  ]

  override navItems: NavItem[] = [
    { name: 'About', fragment: 'about' },
    { name: 'Location', fragment: 'location' },
    { name: 'Events', fragment: 'events' },
    { name: 'Reviews', fragment: 'reviews' },
    { name: 'Listings', fragment: 'listings' }
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
    return this.item;
  }

  @Input()
  set venue(value: Venue | undefined) {
    this.item = value;
  }

  @Output()
  get venueChange() {
    return this.itemChange;
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

  updateLatLong(latLong: google.maps.LatLngLiteral | undefined) {
    if(this.venue)
      if (latLong) {
        const { lat, lng } = latLong;
        this.venue.latitude = lat;
        this.venue.longitude = lng;
    }
    this.onChangeDetected();
  }

  onListingCreate(listing: Listing) {
    this.listingCreate.emit(listing);
  }

  onViewInMaps() {
    if(this.venue?.latitude && this.venue.longitude) {
      const url = `https://www.google.com/maps/search/?api=1&query=${this.venue.latitude},${this.venue.longitude}`;
      window.open(url, '_blank');
    }
  }

  onImageChange(image: File) {
    this.imageChange.emit(image);
    this.onChangeDetected();
  }

}
