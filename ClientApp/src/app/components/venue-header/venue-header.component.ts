import { Component, Input } from '@angular/core';
import { VenueHeader } from '../../models/venue-header';

@Component({
  selector: 'app-venue-header',
  standalone: false,
  
  templateUrl: './venue-header.component.html',
  styleUrl: './venue-header.component.scss'
})
export class VenueHeaderComponent {
  @Input() header?: VenueHeader;
}
