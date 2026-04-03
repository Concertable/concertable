import { Component, ContentChild, Input, TemplateRef } from '@angular/core';
import { HeaderType } from '../../models/header-type';
import { Header } from '../../models/header';
import { EventHeader } from '../../models/event-header';
import { ArtistHeader } from '../../models/artist-header';
import { VenueHeader } from '../../models/venue-header';

@Component({
  selector: 'app-header-carousel',
  standalone: false,
  templateUrl: './header-carousel.component.html',
  styleUrl: './header-carousel.component.scss'
})
export class HeaderCarouselComponent {
  @Input() headers: Header[] = [];
  @Input() title: string = '';

  breakpoints = {
    0: { slidesPerView: 1 },
    600: { slidesPerView: 2 },
    900: { slidesPerView: 3 },
    1200: { slidesPerView: 5 },
    1600: { slidesPerView: 6 },
    2000: { slidesPerView: 7 },
    2400: { slidesPerView: 8 }
  };

  // reference of the child
  @ContentChild(TemplateRef) headerTemplate!: TemplateRef<any>;
}