import { ChangeDetectorRef, Component, Input, OnInit } from '@angular/core';
import { Venue } from '../../models/venue';
import { EventService } from '../../services/event/event.service';
import { Event } from '../../models/event';
import { AuthService } from '../../services/auth/auth.service';
import { ActivatedRoute, Router } from '@angular/router';
import { ItemEventsDirective } from '../../directives/item-events/item-events.directive';
import { Observable } from 'rxjs';
import { Artist } from '../../models/artist';

@Component({
  selector: 'app-venue-events',
  standalone: false,
  
  templateUrl: './venue-events.component.html',
  styleUrl: './venue-events.component.scss'
})
export class VenueEventsComponent extends ItemEventsDirective<Venue> {
  getUpcoming(id: number): Observable<Event[]> {
    console.log("called")
    return this.eventService.getUpcomingByVenueId(id);
  }

  getHistory(id: number): Observable<Event[]> {
    console.log("called2")
    return this.eventService.getHistoryByVenueId(id);
  }

  getUnposted(id: number): Observable<Event[]> {
    return this.eventService.getUnpostedByVenueId(id);
  }

  get venue(): Venue | undefined {
    return this.item;
  }

  @Input()
  set venue(venue: Venue) {
    this.item = venue;
  }
}
