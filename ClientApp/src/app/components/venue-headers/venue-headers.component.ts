import { Component, Input } from '@angular/core';
import { VenueHeader } from '../../models/venue-header';
import { HeadersComponent } from '../headers/headers.component';
import { HeaderType } from '../../models/header-type';

@Component({
  selector: 'app-venue-headers',
  standalone: false,
  
  templateUrl: './venue-headers.component.html',
  styleUrl: './venue-headers.component.scss'
})
export class VenueHeadersComponent extends HeadersComponent<VenueHeader> {
  headerType: HeaderType = 'venue'
}
