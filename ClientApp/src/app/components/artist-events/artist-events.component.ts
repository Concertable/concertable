import { Component, Input } from '@angular/core';
import { ItemEventsDirective } from '../../directives/item-events/item-events.directive';
import { Artist } from '../../models/artist';
import { Observable } from 'rxjs';
import { Event } from '../../models/event';

@Component({
  selector: 'app-artist-events',
  standalone: false,
  templateUrl: './artist-events.component.html',
  styleUrl: './artist-events.component.scss'
})
export class ArtistEventsComponent extends ItemEventsDirective<Artist> {
  getUpcoming(id: number): Observable<Event[]> {
    return this.eventService.getUpcomingByArtistId(id);
  }

  getHistory(id: number): Observable<Event[]> {
    return this.eventService.getHistoryByArtistId(id);
  }

  getUnposted(id: number): Observable<Event[]> {
    return this.eventService.getUnpostedByArtistId(id);
  }

  get artist(): Artist | undefined {
    return this.item;
  }

  @Input()
  set artist(artist: Artist) {
    this.item = artist;
  }
}
