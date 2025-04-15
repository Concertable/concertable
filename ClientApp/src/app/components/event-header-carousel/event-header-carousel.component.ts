import { Component, Input } from '@angular/core';
import { EventHeader } from '../../models/event-header';
import { HeaderCarouselDirective } from '../../directives/header-carousel.directive';
import { EventService } from '../../services/event/event.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-event-header-carousel',
  standalone: false,
  templateUrl: './event-header-carousel.component.html',
  styleUrl: './event-header-carousel.component.scss'
})
export class EventHeaderCarouselComponent extends HeaderCarouselDirective<EventHeader> {
  @Input() title: string = '';
  @Input() declare headers: EventHeader[] = [];

  constructor(private venueService: EventService) {
    super();
  }

  getByAmount(amount: number): Observable<EventHeader[]> {
    return this.venueService.getHeadersByAmount(amount);
  }
}
