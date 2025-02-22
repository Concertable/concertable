import { Component } from '@angular/core';
import { Venue } from '../../models/venue';
import { VenueService } from '../../services/venue/venue.service';
import { VenueToastService } from '../../services/toast/venue/venue-toast.service';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-create-venue',
  standalone: false,
  
  templateUrl: './create-venue.component.html',
  styleUrl: './create-venue.component.scss'
})
export class CreateVenueComponent {
  constructor(
    private venueService: VenueService, 
    private venueToastService: VenueToastService,
    private router: Router,
    private route: ActivatedRoute
  ) { }

  venue: Venue = {
    id: 0, 
    type: 'venue', 
    name: "",
    about: "",
    latitude: 0,
    longitude: 0,
    imageUrl: "", 
    county: "",
    town: "",
    approved: false
  }

  createVenue() {
    this.venueService.create(this.venue).subscribe({
      next: (venue) => {
        this.venueToastService.showCreated(venue.name); 
        this.router.navigate(['../my'], { relativeTo: this.route });
      },
      error: (err) => {
        this.venueToastService.showError(err.error.message || "Failed to create venue.");
      }
    });
  }

}
