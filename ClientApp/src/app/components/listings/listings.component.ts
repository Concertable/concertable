import { AfterViewInit, Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { ListingService } from '../../services/listing/listing.service';
import { Venue } from '../../models/venue';
import { of } from 'rxjs';
import { Listing } from '../../models/listing';
import { AuthService } from '../../services/auth/auth.service';
import { ListingApplicationService } from '../../services/listing-application/listing-application.service';

@Component({
  selector: 'app-listings',
  standalone: false,
  templateUrl: './listings.component.html',
  styleUrl: './listings.component.scss'
})
export class ListingsComponent  implements OnChanges {

  @Input() venue?: Venue
  @Input() editMode?: boolean

  listings? : Listing[];

  constructor(private listingService: ListingService, private applicationService: ListingApplicationService, protected authService: AuthService) { }

  ngOnChanges(changes: SimpleChanges): void {
    if(changes['venue'])
      this.getListings();
  }

  getListings() {
    console.log("getActiveListings");
    if(this.venue && this.venue?.id)
      this.listingService.getActiveByVenueId(this.venue.id).subscribe((listings) => this.listings = listings)
  }

  createListing(listing: Listing) {
    this.listingService.create(listing).subscribe();
  }

  onDelete(listing: Listing) {

  }

  onApply(listing: Listing) {
    console.log("applying");
    if(listing.id)
      this.applicationService.applyForListingAsync(listing.id).subscribe();
  }
  
}
