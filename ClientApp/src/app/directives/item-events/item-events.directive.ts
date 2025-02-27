import { Directive, Input, OnInit } from '@angular/core';
import { Artist } from '../../models/artist';
import { Venue } from '../../models/venue';
import { EventService } from '../../services/event/event.service';
import { AuthService } from '../../services/auth/auth.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { Event } from '../../models/event';

@Directive({
  selector: '[appItemEvents]',
  standalone: false
})
export abstract class ItemEventsDirective<T extends Venue | Artist> implements OnInit{
  @Input() item?: T
  events: Event[] = [];

  constructor(
      protected eventService: EventService, 
      protected authService: AuthService,
      private router: Router,
      private route: ActivatedRoute
    ) { }
    
  ngOnInit(): void {
    if(this.item) {
      this.getUpcoming(this.item.id).subscribe((events) => this.events = events);
    }
  }

  abstract getUpcoming(id: number): Observable<Event[]>;
}
