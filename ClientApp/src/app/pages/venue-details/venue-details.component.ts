import { Component, Input, OnInit } from '@angular/core';
import { Venue } from '../../models/venue';
import { VenueService } from '../../services/venue/venue.service';
import { NavItem } from '../../models/nav-item';
import { AuthService } from '../../services/auth/auth.service';
import { ActivatedRoute, Router } from '@angular/router';
import { CoreEntityDetailsDirective } from '../../directives/core-entity-details/core-entity-details.directive';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-venue-details',
  standalone: false,
  
  templateUrl: './venue-details.component.html',
  styleUrl: './venue-details.component.scss'
})
export class VenueDetailsComponent extends CoreEntityDetailsDirective<Venue> {
  @Input('venue') declare entity?: Venue;

  constructor(
    private venueService: VenueService, 
    authService: AuthService, 
    route: ActivatedRoute,
    router: Router
  ) {
    super(authService, route, router);
  }

  get venue(): Venue | undefined {
    return this.entity;
  }

  set venue(value: Venue | undefined) {
    this.entity = value;
  }

  override ngOnInit() {
    if(this.authService.isNotRole('Customer')) {
      this.navItems.push({ name: 'Listings', fragment: 'listings' })
    }
    super.ngOnInit();
  }

  override loadDetails(id: number): Observable<Venue> {
    return this.venueService.getDetailsById(id);
  }

}
