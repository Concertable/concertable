import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { ListingApplicationService } from '../../services/listing-application/listing-application.service';
import { map, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { ToastService } from '../../services/toast/toast.service';
import { HttpErrorResponse } from '@angular/common/http';
import { Location } from '@angular/common';
import { NavigationService } from '../../services/navigation/navigation.service';

export const listingApplicationCheckoutGuard: CanActivateFn = (route, state) => {
  console.log(route.paramMap);
  const id = Number(route.paramMap.get('id'));
  const listingApplicationService = inject(ListingApplicationService);
  const toastService = inject(ToastService);
  const navService = inject(NavigationService);
  const router = inject(Router);

  return listingApplicationService.canAcceptApplication(id).pipe(
    map((response) => {
      return response;
    }),
    catchError((error: HttpErrorResponse) => {
      console.log(error);
      toastService.showErrorResponse(error);
      navService.navigateToPreviousUrl();
      return of(false);
    })
  );
};
