import { Component, Input, OnInit } from '@angular/core';
import { Preference } from '../../models/preference';
import { Genre } from '../../models/genre';
import { GenreService } from '../../services/genre/genre.service';
import { ActivatedRoute, Router } from '@angular/router';
import { DetailsDirective } from '../../directives/details/details.directive';
import { AuthService } from '../../services/auth/auth.service';

@Component({
  selector: 'app-preference-details',
  standalone: false,
  templateUrl: './preference-details.component.html',
  styleUrl: './preference-details.component.scss'
})
export class PreferenceDetailsComponent extends DetailsDirective<Preference> {
  genres: Genre[] = [];

  constructor(
    private genreService: GenreService,
    authService: AuthService,
    route: ActivatedRoute,
    router: Router) 
  { 
    super(authService, route, router)
  }
  
  get preferences(): Preference | undefined {
      return this.entity;
    }
  
    @Input()
    set preferences(preferences: Preference | undefined) {
      this.entity = preferences;
    }

  override ngOnInit(): void {
    super.ngOnInit();
    this.genreService.getAll().subscribe(g => this.genres = g);
  }

  setDetails(data: any): void {
    this.preferences = data['preferences'];
  }
}
