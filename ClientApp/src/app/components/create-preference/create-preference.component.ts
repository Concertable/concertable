import { Component } from '@angular/core';
import { CreateItemDirective } from '../../directives/create-item/create-item.directive';
import { Preference } from '../../models/preference';
import { firstValueFrom, Observable } from 'rxjs';
import { PreferenceService } from '../../services/preference/preference.service';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../services/auth/auth.service';
import { ToastService } from '../../services/toast/toast.service';
import { PreferenceToastService } from '../../services/toast/preference-toast.service';

@Component({
  selector: 'app-create-preference',
  standalone: false,
  templateUrl: './create-preference.component.html',
  styleUrl: './create-preference.component.scss'
})
export class CreatePreferenceComponent extends CreateItemDirective<Preference> {
  constructor(
    private preferenceService: PreferenceService,
    private authService: AuthService,
    private preferenceToastService: PreferenceToastService,
    router: Router,
    route: ActivatedRoute
  ) {
    super(router, route);
   }

  get preference(): Preference | undefined {
      return this.item;
    }
  
    set preference(value: Preference | undefined) {
      this.item = value;
    }
   

   createDefaultItem(): void {
    firstValueFrom(this.authService.currentUser$).then(user => {
      if (!user) {
        this.preferenceToastService.showError('Unable to create preferences. User not found.');
        return;
      }
  
      this.preference = {
        id: 0,
        user: user,
        genres: [],
        radiusKm: 10
      };
    }).catch(error => {
      this.preferenceToastService.showError('Failed to fetch user.');
      console.error(error);
    });
  }
  

  create(preference: Preference): Observable<Preference> {
    return this.preferenceService.create(preference);
  }
  
  showCreated(preference: Preference): void {
    this.preferenceToastService.showCreated();
  }

  override navigateToItem(): void {
    this.router.navigate(['..'], { relativeTo: this.route });
  }
}
