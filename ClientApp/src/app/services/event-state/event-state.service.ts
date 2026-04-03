import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Event } from '../../models/event';

@Injectable({
  providedIn: 'root'
})
export class EventStateService {
  private eventSubject = new BehaviorSubject<Event | undefined>(undefined);

  event$ = this.eventSubject.asObservable();

  get event() : Event | undefined {
    return this.eventSubject.value;
  }

  set event(event: Event | undefined) {
    this.eventSubject.next(event);
  }
}
