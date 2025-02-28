import { AfterViewInit, Component, OnInit } from '@angular/core';
import { VenueService } from '../../services/venue/venue.service';
import { Venue } from '../../models/venue';
import { AuthService } from '../../services/auth/auth.service';
import { cloneDeep } from 'lodash';
import { MyItemDirective } from '../../directives/my-item/my-item.directive';
import { Observable } from 'rxjs';
import { VenueToastService } from '../../services/toast/venue/venue-toast.service';
import { Listing } from '../../models/listing';
import { ListingService } from '../../services/listing/listing.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-my-venue',
  standalone: false,
  
  templateUrl: './my-venue.component.html',
  styleUrl: './my-venue.component.scss'
})
export class MyVenueComponent extends MyItemDirective<Venue> {
  newListings: Listing[] = [];

  constructor(
    route: ActivatedRoute,
    private venueService: VenueService, 
    private venueToastService: VenueToastService,
    private listingService: ListingService) {
    super(route);
  }

  get venue(): Venue | undefined {
    return this.item;
  }

  set venue(value: Venue | undefined) {
    this.item = value;
  }

  setDetails(data: any) {
    this.venue = data['venue'];
  }

  update(venue: Venue): Observable<Venue> {
    return this.venueService.update(venue);
  }

  showUpdated(venue: Venue) {
    this.venueToastService.showUpdated(venue.name);
  }

  addListing(listing: Listing) {
    this.newListings.push(listing);
    this.saveable = true;
  }

  override saveChanges(): void {
      super.saveChanges();
      this.listingService.createMultiple(this.newListings).subscribe(l => this.newListings = []);
  }
}
