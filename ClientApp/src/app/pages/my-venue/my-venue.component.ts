import { AfterViewInit, Component, OnInit } from '@angular/core';
import { VenueService } from '../../services/venue/venue.service';
import { Venue } from '../../models/venue';
import { AuthService } from '../../services/auth/auth.service';
import { cloneDeep } from 'lodash';
import { MyItemDirective } from '../../directives/my-item/my-item.directive';
import { Observable } from 'rxjs';
import { VenueToastService } from '../../services/toast/venue/venue-toast.service';

@Component({
  selector: 'app-my-venue',
  standalone: false,
  
  templateUrl: './my-venue.component.html',
  styleUrl: './my-venue.component.scss'
})
export class MyVenueComponent extends MyItemDirective<Venue> {
  constructor(private venueService: VenueService, private venueToastService: VenueToastService) {
    super();
  }

  get venue(): Venue | undefined {
    return this.item;
  }

  set venue(value: Venue | undefined) {
    this.item = value;
  }

  getDetailsForCurrentUser() : Observable<Venue> {
    return this.venueService.getDetailsForCurrentUser();
  }

  update(venue: Venue): Observable<Venue> {
    console.log("hh")
    return this.venueService.update(venue);
  }

  showUpdated(name: string) {
    this.venueToastService.showUpdated(name);
  }
}
