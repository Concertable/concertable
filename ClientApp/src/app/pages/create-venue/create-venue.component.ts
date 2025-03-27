import { Component } from '@angular/core';
import { Venue } from '../../models/venue';
import { VenueService } from '../../services/venue/venue.service';
import { VenueToastService } from '../../services/toast/venue/venue-toast.service';
import { ActivatedRoute, Router } from '@angular/router';
import { CreateItemDirective } from '../../directives/create-item/create-item.directive';
import { Observable } from 'rxjs';
import { ListingService } from '../../services/listing/listing.service';
import { Listing } from '../../models/listing';

@Component({
  selector: 'app-create-venue',
  standalone: false,
  
  templateUrl: './create-venue.component.html',
  styleUrl: './create-venue.component.scss'
})
export class CreateVenueComponent extends CreateItemDirective<Venue> {
  newListings: Listing[] = [];

  constructor(
    private venueService: VenueService, 
    private venueToastService: VenueToastService,
    private listingService: ListingService,
    router: Router,
    route: ActivatedRoute
  ) {
    super(router, route);
  }

  get venue(): Venue | undefined {
        return this.item;
      }
    
  set venue(value: Venue | undefined) {
    this.item = value;
  }

  createDefaultItem(): void {
      this.venue = {
        id: 0, 
        type: 'venue', 
        name: "",
        about: "",
        latitude: 0,
        longitude: 0,
        imageUrl: "", 
        county: "",
        town: "",
        userId: 0,
        approved: false
      }
  }

  create(venue: Venue): Observable<Venue> {
    return this.venueService.create(venue);
  }

  showCreated(name: string): void {
    this.venueToastService.showCreated(name); 
  }

  addListing(listing: Listing) {
    this.newListings.push(listing);
    //this.saveable = true;
  }

  override createChanges(): void {
      super.createChanges()
      this.listingService.createMultiple(this.newListings).subscribe(l => this.newListings = []);
  }
  
}
