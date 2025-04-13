import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';
import { AuthService } from '../../services/auth/auth.service';
import { ToastService } from '../../services/toast/toast.service';
import { Location } from '@angular/common';
import { NavigationService } from '../../services/navigation/navigation.service';

export const roleGuard: CanActivateFn = (route) => {
  const authService = inject(AuthService);
  const toastService = inject(ToastService);
  const navService = inject(NavigationService);
  const location = inject(Location);
  const router = inject(Router);
  const role = route.data['role'];

  return authService.currentUser$.pipe(
    map(currentUser => {
      if(["Admin", role].includes(currentUser?.role)){
        return true;
      } else {
        toastService.showError("You do not have the role to access this page", "401 Unauthorized");
        const previousUrl = navService.getPreviousUrl();
        router.navigateByUrl(previousUrl);
        return false;
      }
    }),
    catchError((err) => {
      return of(false);
    })
  )
};

