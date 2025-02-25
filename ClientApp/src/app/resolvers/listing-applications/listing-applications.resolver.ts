import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Observable, throwError } from 'rxjs';
import { Artist } from '../../models/artist';
import { ArtistService } from '../../services/artist/artist.service';
import { ListingApplication } from '../../models/listing-application';
import { ListingApplicationService } from '../../services/listing-application/listing-application.service';

@Injectable({
  providedIn: 'root'
})
export class ListingApplicationsResolver implements Resolve<ListingApplication[]> {

  constructor(private listingApplicationService: ListingApplicationService) {}

  resolve(route: ActivatedRouteSnapshot): Observable<ListingApplication[]> {
    const id = route.paramMap.get('id');
    return this.listingApplicationService.getAllForListingId(Number(id));
  }
}
