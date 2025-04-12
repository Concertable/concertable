import { Component, Input } from '@angular/core';
import { VenueHeader } from '../../models/venue-header';
import { HeaderCarouselDirective } from '../../directives/header-carousel.directive';
import { Observable } from 'rxjs';
import { VenueService } from '../../services/venue/venue.service';

@Component({
  selector: 'app-venue-header-carousel',
  standalone: false,
  templateUrl: './venue-header-carousel.component.html',
  styleUrl: './venue-header-carousel.component.scss'
})
export class VenueHeaderCarouselComponent extends HeaderCarouselDirective<VenueHeader> {
  @Input() declare headers;
  @Input() title: string = '';
  
    constructor(private venueService: VenueService) {
      super();
    }
  
    getByAmount(amount: number): Observable<VenueHeader[]> {
        return this.venueService.getHeadersByAmount(amount);
    }
}
