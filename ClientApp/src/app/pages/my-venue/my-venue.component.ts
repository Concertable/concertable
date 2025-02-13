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

@Component({
  selector: 'app-my-venue',
  standalone: false,
  
  templateUrl: './my-venue.component.html',
  styleUrl: './my-venue.component.scss'
})
export class MyVenueComponent extends MyItemDirective<Venue> {
  newListings: Listing[] = []

  constructor(
    private venueService: VenueService, 
    private venueToastService: VenueToastService,
    private listingService: ListingService) {
    super();
  }

  get venue(): Venue | undefined {
    return this.item;
  }

  set venue(value: Venue | undefined) {
    this.item = value;
  }

  getDetails() : Observable<Venue> {
    return this.venueService.getDetailsForCurrentUser();
  }

  update(venue: Venue): Observable<Venue> {
    console.log("hh")
    return this.venueService.update(venue);
  }

  showUpdated(name: string) {
    this.venueToastService.showUpdated(name);
  }

  addListing(listing: Listing) {
    this.newListings.push(listing);
    this.saveable = true;
  }

  override saveChanges(): void {
      super.saveChanges();
      this.listingService.createMultiple(this.newListings).subscribe();
  }
}
