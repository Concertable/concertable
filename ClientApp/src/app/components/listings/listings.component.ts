import { AfterViewInit, Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { ListingService } from '../../services/listing/listing.service';
import { Venue } from '../../models/venue';
import { of } from 'rxjs';
import { Listing } from '../../models/listing';
import { AuthService } from '../../services/auth/auth.service';

@Component({
  selector: 'app-listings',
  standalone: false,
  templateUrl: './listings.component.html',
  styleUrl: './listings.component.scss'
})
export class ListingsComponent  implements OnChanges {

  @Input() venue?: Venue
  @Input() editMode?: boolean
  protected listings?: Listing[]

  constructor(private listingService: ListingService, protected authService: AuthService) { }

  ngOnChanges(changes: SimpleChanges): void {
    if(changes['venue'])
      this.getListings();
  }

  getListings() {
    console.log("getActiveListings");
    if(this.venue?.id) 
      this.listingService.getActiveListingsByVenueId(this.venue.id).subscribe((listingsResponse) => {console.log("y",listingsResponse); this.listings = listingsResponse})
  }

  createListing(listing: Listing) {
    this.listingService.createListing(listing).subscribe();
  }

  onDelete(listing: Listing) {

  }
  
}
