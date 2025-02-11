import { Component, Input } from '@angular/core';
import { NavItem } from '../../models/nav-item';
import { VenueService } from '../../services/venue/venue.service';
import { AuthService } from '../../services/auth/auth.service';
import { ActivatedRoute, Router } from '@angular/router';
import { EventService } from '../../services/event/event.service';
import { Event } from '../../models/event';

@Component({
  selector: 'app-event-details',
  standalone: false,
  
  templateUrl: './event-details.component.html',
  styleUrl: './event-details.component.scss'
})
export class EventDetailsComponent {
  @Input() event?: Event;
  @Input() editMode?: boolean
  
    navItems: NavItem[] = [
      { name: 'Info', fragment: 'info' },
      { name: 'Events', fragment: 'events' },
      { name: 'Videos', fragment: 'videos' },
      { name: 'Reviews', fragment: 'reviews' }
  ];
  
    
    constructor(
      private eventService: EventService, 
      protected authService: AuthService, 
      private route: ActivatedRoute,
      private router: Router) { }
  
    ngOnInit() {
      if(this.authService.isNotRole('Customer')) {
        this.navItems.push({ name: 'Listings', fragment: 'listings' })
      }
      if (!this.event) {
        console.log(this.router.url)
          this.route.queryParams.subscribe(params => {
            const eventId = params['id'];
            if (eventId) {
              this.eventService.getDetailsById(eventId).subscribe(event => this.event=event);
            }
          });
      }
    }
  
    exists(s: string): boolean {
      return this.navItems.some(n => n.name === s)
    }

}
