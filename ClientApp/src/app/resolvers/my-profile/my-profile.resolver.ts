import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Observable, of } from 'rxjs';
import { User } from '../../models/user';
import { AuthService } from '../../services/auth/auth.service';

@Injectable({
  providedIn: 'root'
})
export class MyProfileResolver implements Resolve<User | undefined> {

  constructor(private authService: AuthService) {}

  resolve(): Observable<User | undefined> {
    return this.authService.currentUser$;
  }  
}
