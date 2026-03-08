import { Component, Input } from '@angular/core';
import { ArtistHeader } from '../../models/artist-header';
import { HeaderCarouselDirective } from '../../directives/header-carousel.directive';
import { HeaderService, HEADER_TYPE } from '../../services/header/header.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-artist-header-carousel',
  standalone: false,
  templateUrl: './artist-header-carousel.component.html',
  styleUrl: './artist-header-carousel.component.scss',
  providers: [
    HeaderService,
    { provide: HEADER_TYPE, useValue: 'artist' }
  ]
})
export class ArtistHeaderCarouselComponent extends HeaderCarouselDirective<ArtistHeader> {
  @Input() declare headers: ArtistHeader[];
  @Input() title: string = '';

  constructor(private headerService: HeaderService) {
    super();
  }

  getByAmount(amount: number): Observable<ArtistHeader[]> {
    return this.headerService.getByAmount(amount);
  }
}
