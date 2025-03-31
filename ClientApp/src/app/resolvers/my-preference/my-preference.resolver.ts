import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Observable, of } from 'rxjs';
import { switchMap, take } from 'rxjs/operators';
import { User } from '../../models/user';
import { AuthService } from '../../services/auth/auth.service';
import { Preference } from '../../models/preference';
import { PreferenceService } from '../../services/preference/preference.service';

@Injectable({
  providedIn: 'root'
})
export class myPreferenceResolver implements Resolve<Preference> {

  constructor(private preferenceService: PreferenceService) {}

  resolve(): Observable<Preference> {
    return this.preferenceService.getByUser();
  }  
}
