import { Component, Input } from '@angular/core';
import { ArtistHeader } from '../../models/artist-header';
import { HeaderCarouselDirective } from '../../directives/header-carousel.directive';
import { Observable } from 'rxjs';
import { ArtistService } from '../../services/artist/artist.service';

@Component({
  selector: 'app-artist-header-carousel',
  standalone: false,
  templateUrl: './artist-header-carousel.component.html',
  styleUrl: './artist-header-carousel.component.scss'
})
export class ArtistHeaderCarouselComponent extends HeaderCarouselDirective<ArtistHeader> {
  @Input() declare headers;
  @Input() title: string = '';

  constructor(private artistService: ArtistService) {
    super();
    
  }

  getByAmount(amount: number): Observable<ArtistHeader[]> {
      return this.artistService.getHeadersByAmount(amount);
  }
}
