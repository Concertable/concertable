import { Component, OnInit } from '@angular/core';
import { PreferenceService } from '../../services/preference/preference.service';
import { Preference } from '../../models/preference';
import { ActivatedRoute } from '@angular/router';
import { GenreService } from '../../services/genre/genre.service';
import { Genre } from '../../models/genre';
import { ConfigDirective } from '../../directives/config/config.directive';
import { Observable } from 'rxjs';
import { PreferenceToastService } from '../../services/toast/preference-toast.service';

@Component({
  selector: 'app-my-preferences',
  standalone: false,
  templateUrl: './my-preferences.component.html',
  styleUrl: './my-preferences.component.scss'
})
export class MyPreferencesComponent extends ConfigDirective<Preference> {
  constructor(
    route: ActivatedRoute,
    private preferenceService: PreferenceService,
    private preferenceToastService: PreferenceToastService
  ) {
    super(route);
  }

  get preferences(): Preference | undefined {
      return this.item;
    }
  
  set preferences(value: Preference | undefined) {
    this.item = value;
  }

  setDetails(data: any): void {
    this.preferences = data['preferences']; 
  }

  update(preference: Preference): Observable<Preference> {
    return this.preferenceService.update(preference);
  }

  showUpdated(preference: Preference): void {
    this.preferenceToastService.showUpdated();
  }
}
