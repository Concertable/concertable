import { Component, Input } from '@angular/core';
import { HeaderType } from '../../models/header-type';
import { Header } from '../../models/header';
import { EventHeader } from '../../models/event-header';

@Component({
  selector: 'app-event-carousel',
  standalone: false,
  templateUrl: './event-carousel.component.html',
  styleUrl: './event-carousel.component.scss'
})
export class EventCarouselComponent {
  @Input() headers: EventHeader[] = [];
  @Input() title: string = '';

  breakpoints = {
    0: {
      slidesPerView: 1,
    },
    600: {
      slidesPerView: 2,
    },
    900: {
      slidesPerView: 3,
    },
    1200: {
      slidesPerView: 5,
    },
    1600: {
      slidesPerView: 6,
    },
    2000: {
      slidesPerView: 7,
    },
    2400: {
      slidesPerView: 8,
    }
  };
  
}
