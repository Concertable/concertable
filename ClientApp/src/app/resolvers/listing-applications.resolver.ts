import { Injectable } from '@angular/core';
import { Resolve, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { ListingApplication } from '../models/listing-application';
import { ListingApplicationService } from '../services/listing-application/listing-application.service';

@Injectable({ providedIn: 'root' })
export class ListingApplicationsResolver implements Resolve<ListingApplication[]> {
  constructor(private listingApplicationService: ListingApplicationService) {}

  resolve(route: ActivatedRouteSnapshot): Observable<ListingApplication[]> {
    const id = route.paramMap.get('id');
    return this.listingApplicationService.getAllForListingId(Number(id));
  }
}
