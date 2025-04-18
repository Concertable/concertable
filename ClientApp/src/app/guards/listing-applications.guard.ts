import { CanActivateFn } from '@angular/router';
import { inject } from '@angular/core';
import { VenueService } from '../services/venue/venue.service';
import { ToastService } from '../services/toast/toast.service';
import { map, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { ListingApplicationService } from '../services/listing-application/listing-application.service';
import { ListingService } from '../services/listing/listing.service';

export const listingApplicationsGuard: CanActivateFn = (route, state) => {
  const listingService = inject(ListingService);
  const toastService = inject(ToastService);
  const applicationId = Number(route.paramMap.get('id'));

  return listingService.isOwner(applicationId).pipe(
    map(isOwner => {
      if (!isOwner) toastService.showError('You do not own this Venue');
      return isOwner;
    }),
    catchError(() => {
      toastService.showError('An error occurred while checking ownership');
      return of(false);
    })
  );
};
