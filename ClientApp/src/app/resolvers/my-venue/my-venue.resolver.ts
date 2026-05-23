import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Observable, throwError } from 'rxjs';
import { Venue } from '../../models/venue';
import { VenueService } from '../../services/venue/venue.service';

@Injectable({
  providedIn: 'root'
})
export class MyVenueResolver implements Resolve<Venue> {

  constructor(private venueService: VenueService) {}

  resolve(): Observable<Venue> {
    return this.venueService.getDetailsForCurrentUser();
  }
}
