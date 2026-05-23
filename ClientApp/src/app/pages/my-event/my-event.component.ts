import { Component } from '@angular/core';
import { ConfigDirective } from '../../directives/config/config.directive';
import { Event } from '../../models/event';
import { Observable, of, switchMap, throwError } from 'rxjs';
import { EventService } from '../../services/event/event.service';
import { ActivatedRoute } from '@angular/router';
import { cloneDeep } from 'lodash';
import { EventToastService } from '../../services/toast/event/event-toast.service';

@Component({
  selector: 'app-my-event',
  standalone: false,
  templateUrl: './my-event.component.html',
  styleUrl: './my-event.component.scss'
})
export class MyEventComponent extends ConfigDirective<Event> {

  constructor(
      route: ActivatedRoute,
      private eventService: EventService,
      private eventToastService: EventToastService) {
      super(route);
    }

  get event() : Event | undefined {
    return this.item;
  }

  set event(item: Event) {
    this.item = item;
  }
    
  setDetails(data: any): void {
    this.event = data['event'];
  }

  update(event: Event): Observable<Event> {
    if(this.item?.datePosted)
      return this.eventService.update(event);
    return this.eventService.post(event);
  }

  showUpdated(event: Event): void {
    this.eventToastService.showUpdated()
  }

}
