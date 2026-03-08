import { Component, Input } from '@angular/core';
import { VenueHeader } from '../../models/venue-header';
import { HeaderCarouselDirective } from '../../directives/header-carousel.directive';
import { HeaderService, HEADER_TYPE } from '../../services/header/header.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-venue-header-carousel',
  standalone: false,
  templateUrl: './venue-header-carousel.component.html',
  styleUrl: './venue-header-carousel.component.scss',
  providers: [
    HeaderService,
    { provide: HEADER_TYPE, useValue: 'venue' }
  ]
})
export class VenueHeaderCarouselComponent extends HeaderCarouselDirective<VenueHeader> {
  @Input() declare headers: VenueHeader[];
  @Input() title: string = '';

  constructor(private headerService: HeaderService) {
    super();
  }

  getByAmount(amount: number): Observable<VenueHeader[]> {
    return this.headerService.getByAmount(amount);
  }
}
