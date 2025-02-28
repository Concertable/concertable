import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Observable, of } from 'rxjs';
import { switchMap, take } from 'rxjs/operators';
import { User } from '../../models/user';
import { AuthService } from '../../services/auth/auth.service';

@Injectable({
  providedIn: 'root'
})
export class MyProfileResolver implements Resolve<User | undefined> {

  constructor(private authService: AuthService) {}

  /*
  * If the user has already been loaded through the init service, get that user
  * otherwise, if they havent yet (due to navigate to this page immediately)
  * then get the user from the database
  */
  resolve(): Observable<User | undefined> {
    if(this.authService.currentUserLoaded)
      return this.authService.currentUser$;
    return this.authService.getCurrentUser();
  }  
}
