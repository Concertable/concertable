import { Directive, Input, OnInit } from '@angular/core';
import { Artist } from '../../models/artist';
import { Venue } from '../../models/venue';
import { EventService } from '../../services/event/event.service';
import { AuthService } from '../../services/auth/auth.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { Event } from '../../models/event';
import { EventViewType } from '../../models/event-view-type';

@Directive({
  selector: '[appItemEvents]',
  standalone: false
})
export abstract class ItemEventsDirective<T extends Venue | Artist> implements OnInit{
  @Input() item?: T
  events: Event[] = [];
  @Input() viewType?: EventViewType;

  constructor(
      protected eventService: EventService, 
      protected authService: AuthService,
      private router: Router,
      private route: ActivatedRoute
    ) { }
    
  ngOnInit(): void {
    let method: ((id: number) => Observable<Event[]>) | undefined;


    switch (this.viewType) {
      case EventViewType.Upcoming:
        method = this.getUpcoming;
        break;
      case EventViewType.History:
        method = this.getHistory;
        break;
      case EventViewType.Unposted:
        method = this.getUnposted;
        break;
    }

    if(this.item && method)
      method.call(this, this.item.id).subscribe(events => this.events = events);

  }

  abstract getUpcoming(id: number): Observable<Event[]>;

  abstract getHistory(id: number): Observable<Event[]>;

  abstract getUnposted(id: number): Observable<Event[]>;
}
