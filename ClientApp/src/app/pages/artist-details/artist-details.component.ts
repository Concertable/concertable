import { Component, Input, OnInit } from '@angular/core';
import { Artist } from '../../models/artist';
import { ArtistService } from '../../services/artist/artist.service';
import { ActivatedRoute, Router } from '@angular/router';
import { CoreEntityDetailsDirective } from '../../directives/core-entity-details/core-entity-details.directive';
import { AuthService } from '../../services/auth/auth.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-artist-details',
  standalone: false,
  
  templateUrl: './artist-details.component.html',
  styleUrl: './artist-details.component.scss'
})
export class ArtistDetailsComponent extends CoreEntityDetailsDirective<Artist> {
  @Input('artist') declare entity?: Artist;

  constructor(
    private artistService: ArtistService,
    authService: AuthService,
    route: ActivatedRoute,
    router: Router) 
  { 
    super(authService, route, router)
  }

  get artist(): Artist | undefined {
    return this.entity;
  }

  set artist(value: Artist | undefined) {
    this.entity = value;
  }

  override ngOnInit(): void {
      super.ngOnInit();
  }

  loadDetails(id: number): Observable<Artist> {
    return this.artistService.getDetailsById(id);
  }
}
