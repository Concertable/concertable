import { Component, Input, OnInit } from '@angular/core';
import { Artist } from '../../models/artist';
import { ArtistService } from '../../services/artist/artist.service';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../services/auth/auth.service';
import { Observable } from 'rxjs';
import { DetailsDirective } from '../../directives/details/details.directive';
import { BlobStorageService } from '../../services/blob-storage/blob-storage.service';
import { ExtendedDetailsDirective } from '../../directives/extended-details.directive';
import { Genre } from '../../models/genre';
import { GenreService } from '../../services/genre/genre.service';
import { NavItem } from '../../models/nav-item';

@Component({
  selector: 'app-artist-details',
  standalone: false,
  
  templateUrl: './artist-details.component.html',
  styleUrl: './artist-details.component.scss'
})
export class ArtistDetailsComponent extends ExtendedDetailsDirective<Artist> {
  genres: Genre[] = [];

  override navItems: NavItem[] = [
      { name: 'Info', fragment: 'info' },
      { name: 'Upcoming Events', fragment: 'upcoming-events' },
      { name: 'Videos', fragment: 'videos' },
      { name: 'Reviews', fragment: 'reviews' }
    ];

  constructor(
    private artistService: ArtistService,
    private genreService: GenreService,
    authService: AuthService,
    blobStorageService: BlobStorageService,
    route: ActivatedRoute,
    router: Router) 
  { 
    super(blobStorageService, authService, route, router)
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
      this.genreService.getAll().subscribe(g => this.genres = g)
  }

  setDetails(data: any): void {
    this.artist = data['artist'];
  }

  updateImage(url: string) {
    if(this.artist)
      this.artist.imageUrl = url
    this.onChangeDetected
  }
}
