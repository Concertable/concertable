import { CanActivateFn, Router } from '@angular/router';
import { TicketService } from '../../services/ticket/ticket.service';
import { ToastService } from '../../services/toast/toast.service';
import { NavigationService } from '../../services/navigation.service';
import { inject } from '@angular/core';
import { catchError, map, of } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';

export const eventCheckoutGuard: CanActivateFn = (route, state) => {
  const id = Number(route.paramMap.get('id'));
  const ticketService = inject(TicketService);
  const toastService = inject(ToastService);
  const navService = inject(NavigationService);

  return ticketService.canPurchase(id).pipe(
    map((response) => {
      return response;
    }),
    catchError((error: HttpErrorResponse) => {
      toastService.showErrorResponse(error);
      navService.navigateToPreviousUrl();
      return of(false);
    })
  );
};
