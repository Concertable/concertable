import { Component, Input } from '@angular/core';
import { NavItem } from '../../models/nav-item';
import { VenueService } from '../../services/venue/venue.service';
import { AuthService } from '../../services/auth/auth.service';
import { ActivatedRoute, Router } from '@angular/router';
import { EventService } from '../../services/event/event.service';
import { Event } from '../../models/event';
import { DetailsDirective } from '../../directives/details/details.directive';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-event-details',
  standalone: false,
  
  templateUrl: './event-details.component.html',
  styleUrl: './event-details.component.scss'
})
export class EventDetailsComponent extends DetailsDirective<Event> {
  @Input('event') declare entity?: Event;

  override navItems: NavItem[] = [
    { name: 'Info', fragment: 'info' },
    { name: 'Location', fragment: 'location' },
    { name: 'Artist', fragment: 'artist' },
    { name: 'Videos', fragment: 'videos' },
    { name: 'Reviews', fragment: 'reviews' }
  ];

  constructor(
    private eventService: EventService,
    authService: AuthService,
    route: ActivatedRoute,
    router: Router) 
  { 
    super(authService, route, router)
  }

  get event(): Event | undefined {
    return this.entity;
  }

  set event(value: Event | undefined) {
    this.entity = value;
  }

  override ngOnInit(): void {
      super.ngOnInit();
  }

  loadDetails(id: number): Observable<Event> {
    return this.eventService.getDetailsById(id);
  }

  onArtistDetailsClick() {
    this.router.navigate(['../artist'], { 
      relativeTo: this.route, 
      queryParams: { id: this.event?.artist.id } 
    });
  }

}
