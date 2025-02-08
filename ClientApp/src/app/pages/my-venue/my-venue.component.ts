import { AfterViewInit, Component, OnInit } from '@angular/core';
import { VenueService } from '../../services/venue/venue.service';
import { Venue } from '../../models/venue';
import { AuthService } from '../../services/auth/auth.service';

@Component({
  selector: 'app-my-venue',
  standalone: false,
  
  templateUrl: './my-venue.component.html',
  styleUrl: './my-venue.component.scss'
})
export class MyVenueComponent implements OnInit {

  protected venue: Venue | undefined;
  protected editMode: boolean = false;
  constructor(protected venueService: VenueService) { }

  onEditModeChange(newValue: boolean) {
    this.editMode = newValue;
  }

  ngOnInit(): void {
    this.venueService.getUserVenue().subscribe((venue) => {
      console.log(venue);
      this.venue = venue
    });
  }
}
