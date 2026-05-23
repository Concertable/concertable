import { Injectable } from '@angular/core';
import { Resolve, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { EventService } from '../../services/event/event.service';
import { Event } from '../../models/event';

@Injectable({ providedIn: 'root' })
export class EventResolver implements Resolve<Event> {
  constructor(private eventService: EventService) {}

  resolve(route: ActivatedRouteSnapshot): Observable<Event> {
    const id = route.paramMap.get('id');
    return this.eventService.getDetailsById(Number(id));
  }
}
