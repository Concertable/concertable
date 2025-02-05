import { Component, Input } from '@angular/core';
import { Venue } from '../../models/venue';
import { VenueService } from '../../services/venue/venue.service';
import { NavItem } from '../../models/nav-item';
import { AuthService } from '../../services/auth/auth.service';

@Component({
  selector: 'app-venue-details',
  standalone: false,
  
  templateUrl: './venue-details.component.html',
  styleUrl: './venue-details.component.scss'
})
export class VenueDetailsComponent {
  @Input() venue?: Venue;
  @Input() editMode?: boolean

  navItems: NavItem[] = [
    { name: 'Section 1', fragment: 'section1' },
    { name: 'Section 2', fragment: 'section2' },
    { name: 'Section 3', fragment: 'section3' }
]

  constructor(private venueService: VenueService, protected authService: AuthService) { }
}
