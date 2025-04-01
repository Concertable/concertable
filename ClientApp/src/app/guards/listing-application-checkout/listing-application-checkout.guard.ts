import { CanActivateFn } from '@angular/router';
import { inject } from '@angular/core';
import { ListingApplicationService } from '../../services/listing-application/listing-application.service';
import { map, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { ToastService } from '../../services/toast/toast.service';
import { HttpErrorResponse } from '@angular/common/http';

export const listingApplicationCheckoutGuard: CanActivateFn = (route, state) => {
  const id = Number(route.params['id']);
  const listingApplicationService = inject(ListingApplicationService);
  const toastService = inject(ToastService);

  return listingApplicationService.canAcceptApplication(id).pipe(
    map((response) => {
      return response;
    }),
    catchError((error: HttpErrorResponse) => {
      toastService.showError(error.error, error.message);
      return of(false);
    })
  );
};
