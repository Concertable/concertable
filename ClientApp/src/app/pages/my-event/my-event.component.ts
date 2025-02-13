import { Component } from '@angular/core';
import { MyItemDirective } from '../../directives/my-item/my-item.directive';
import { Event } from '../../models/event';
import { Observable, of, switchMap, throwError } from 'rxjs';
import { EventService } from '../../services/event/event.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-my-event',
  standalone: false,
  templateUrl: './my-event.component.html',
  styleUrl: './my-event.component.scss'
})
export class MyEventComponent extends MyItemDirective<Event> {

  constructor(
      private eventService: EventService,
      private route: ActivatedRoute) {
      super();
    }

  get event() : Event | undefined {
    return this.item;
  }

  set event(item: Event) {
    this.event = item;
  }

  override getDetails(): Observable<Event> {
    return this.route.queryParams.pipe(
      switchMap(params => {
        const eventId = params['id'];
        return eventId ? this.eventService.getDetailsById(eventId) : throwError(() => new Error("No event ID provided"));
      }) //maps observable to event observable to return it
    );
  }
    

  override update(item: Event): Observable<Event> {
    return of()
  }

  override showUpdated(name: string): void {

  }

}
