import { Component, Input } from '@angular/core';
import { VenueHeader } from '../../models/venue-header';
import { ActivatedRoute, Router } from '@angular/router';
import { HeaderComponent } from '../header/header.component';

@Component({
  selector: 'app-venue-header',
  standalone: false,
  
  templateUrl: './venue-header.component.html',
  styleUrl: './venue-header.component.scss'
})
export class VenueHeaderComponent extends HeaderComponent<VenueHeader> {
}
