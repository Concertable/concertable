import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Observable, throwError } from 'rxjs';
import { VenueService } from '../../services/venue/venue.service';
import { Venue } from '../../models/venue';

@Injectable({
  providedIn: 'root'
})
export class VenueDetailsResolver implements Resolve<Venue> {

  constructor(private venueService: VenueService) {}

  resolve(route: ActivatedRouteSnapshot): Observable<Venue> {
    const id = route.paramMap.get('id');

    if (!id) {
      return throwError(() => new Error("No venue ID provided")); 
    }

    return this.venueService.getDetailsById(Number(id)); 
  }
}
