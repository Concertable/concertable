import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';
import { AuthService } from '../../services/auth/auth.service';
import { ToastService } from '../../services/toast/toast.service';
import { Location } from '@angular/common';

export const roleGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const toastService = inject(ToastService);
  const location = inject(Location);
  const router = inject(Router);
  const role = route.data['role'];

  return authService.currentUser$.pipe(
    map(currentUser => {
      if(["Admin", role].includes(currentUser?.role)){
        return true;
      } else {
        toastService.showError("You do not have the role to access this page", "Unauthorized")
        location.back();
        return false;
      }
    }),
    catchError((err) => {
      console.log(err);
      return of(false);
    })
  )
  
};