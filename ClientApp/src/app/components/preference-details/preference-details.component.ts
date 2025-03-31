import { Component, Input, OnInit } from '@angular/core';
import { Preference } from '../../models/preference';
import { Genre } from '../../models/genre';
import { GenreService } from '../../services/genre/genre.service';
import { ActivatedRoute, Router } from '@angular/router';
import { DetailsDirective } from '../../directives/details/details.directive';
import { AuthService } from '../../services/auth/auth.service';
import { firstValueFrom } from 'rxjs';
import { ToastService } from '../../services/toast/toast.service';

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
    router: Router,
    toastService: ToastService) 
  { 
    super(authService, route, router, toastService)
  }
  
  get preference(): Preference | undefined {
    return this.entity;
  }

  @Input()
  set preference(preferences: Preference | undefined) {
    this.entity = preferences;
  }
  
  override ngOnInit(): void {
    super.ngOnInit();
    this.genreService.getAll().subscribe(g => this.genres = g);
  }

  setDetails(data: any): void {
    this.preference = data['preference'];
  }
}
