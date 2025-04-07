import { Component, Input, OnInit } from '@angular/core';
import { Artist } from '../../models/artist';
import { ArtistService } from '../../services/artist/artist.service';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../services/auth/auth.service';
import { Observable } from 'rxjs';
import { DetailsDirective } from '../../directives/details/details.directive';
import { BlobStorageService } from '../../services/blob-storage/blob-storage.service';
import { ExtendedDetailsDirective } from '../../directives/extended-details/extended-details.directive';
import { Genre } from '../../models/genre';
import { GenreService } from '../../services/genre/genre.service';
import { NavItem } from '../../models/nav-item';
import { ToastService } from '../../services/toast/toast.service';

@Component({
  selector: 'app-artist-details',
  standalone: false,
  
  templateUrl: './artist-details.component.html',
  styleUrl: './artist-details.component.scss'
})
export class ArtistDetailsComponent extends ExtendedDetailsDirective<Artist> {
  override navItems: NavItem[] = [
      { name: 'About', fragment: 'about' },
      { name: 'Upcoming Events', fragment: 'upcoming-events' },
      { name: 'Reviews', fragment: 'reviews' }
    ];

  constructor(
    private artistService: ArtistService,
    authService: AuthService,
    blobStorageService: BlobStorageService,
    genreService: GenreService,
    route: ActivatedRoute,
    router: Router,
    toastService: ToastService) 
  { 
    super(blobStorageService, genreService, authService, route, router, toastService)
  }

  get artist(): Artist | undefined {
    return this.entity;
  }

  @Input()
  set artist(artist: Artist | undefined) {
    console.log("Call");
    this.entity = artist;
  }

  override ngOnInit(): void {
      super.ngOnInit();
  }

  setDetails(data: any): void {
    this.artist = data['artist'];
  }
}
