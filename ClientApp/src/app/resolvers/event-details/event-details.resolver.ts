import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Observable, throwError } from 'rxjs';
import { Event } from '../../models/event';
import { EventService } from '../../services/event/event.service';

@Injectable({
  providedIn: 'root'
})
export class EventDetailsResolver implements Resolve<Event> {

  constructor(private eventService: EventService) {}

  resolve(route: ActivatedRouteSnapshot): Observable<Event> {
    const id = route.paramMap.get('id'); 

    if (!id) {
      return throwError(() => new Error("No artist ID provided"));
    }

    return this.eventService.getDetailsById(Number(id)); 
  }
}
