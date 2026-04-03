import { Component, Input } from '@angular/core';
import { ConcertHeader } from '../../models/concert-header';
import { HeaderCarouselDirective } from '../../directives/header-carousel.directive';
import { HeaderService, HEADER_TYPE } from '../../services/header/header.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-event-header-carousel',
  standalone: false,
  templateUrl: './event-header-carousel.component.html',
  styleUrl: './event-header-carousel.component.scss',
  providers: [
    HeaderService,
    { provide: HEADER_TYPE, useValue: 'concert' }
  ]
})
export class EventHeaderCarouselComponent extends HeaderCarouselDirective<ConcertHeader> {
  @Input() title: string = '';
  @Input() declare headers: ConcertHeader[];

  constructor(private headerService: HeaderService) {
    super();
  }

  getByAmount(amount: number): Observable<ConcertHeader[]> {
    return this.headerService.getByAmount(amount);
  }
}
