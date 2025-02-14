import { Injectable } from '@angular/core';
import { Event } from '../../models/event';

@Injectable({
  providedIn: 'root'
})
export class EventCheckoutService {
  private _event?: Event;

  get event(): Event | undefined {
    return this._event;
  }

  set event(event: Event) {
    this._event = event; 
  }
}
