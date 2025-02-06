import { Component, Input } from '@angular/core';
import { VenueHeader } from '../../models/venue-header';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-venue-header',
  standalone: false,
  
  templateUrl: './venue-header.component.html',
  styleUrl: './venue-header.component.scss'
})
export class VenueHeaderComponent {
  @Input() header?: VenueHeader;

  constructor(private router: Router, private route: ActivatedRoute) { }
}
