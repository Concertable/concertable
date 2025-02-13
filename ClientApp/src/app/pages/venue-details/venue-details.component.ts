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

@Component({
  selector: 'app-venue-details',
  standalone: false,
  
  templateUrl: './venue-details.component.html',
  styleUrl: './venue-details.component.scss'
})
export class VenueDetailsComponent extends DetailsDirective<Venue> {
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
    router: Router
  ) {
    super(authService, route, router);
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

  override loadDetails(id: number): Observable<Venue> {
    return this.venueService.getDetailsById(id);
  }

  updateCoordinates(coordinates: Coordinates) {
    if(this.venue)
      this.venue.coordinates = coordinates;
    this.onChangeDetected();
    console.log(this.venue?.coordinates)
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
