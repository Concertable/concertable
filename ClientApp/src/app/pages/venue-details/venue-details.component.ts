import { Component, Input, OnInit } from '@angular/core';
import { Venue } from '../../models/venue';
import { VenueService } from '../../services/venue/venue.service';
import { NavItem } from '../../models/nav-item';
import { AuthService } from '../../services/auth/auth.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-venue-details',
  standalone: false,
  
  templateUrl: './venue-details.component.html',
  styleUrl: './venue-details.component.scss'
})
export class VenueDetailsComponent implements OnInit {
  @Input() venue?: Venue;
  @Input() editMode?: boolean

  navItems: NavItem[] = [
    { name: 'Info', fragment: 'info' },
    { name: 'Events', fragment: 'events' },
    { name: 'Videos', fragment: 'videos' },
    { name: 'Reviews', fragment: 'reviews' }
];

  
  constructor(private venueService: VenueService, protected authService: AuthService, private route: ActivatedRoute) { }

  ngOnInit() {
    if(this.authService.isNotRole('Customer')) {
      this.navItems.push({ name: 'Listings', fragment: 'listings' })
    }
    if (!this.venue) {
      this.route.queryParams.subscribe(params => {
        const venueId = params['id'];
        if (venueId) {
          this.venueService.getDetailsById(venueId).subscribe(venue => this.venue=venue);
        }
      });
    }
  }

  exists(s: string): boolean {
    return this.navItems.some(n => n.name === s)
  }
  

}
