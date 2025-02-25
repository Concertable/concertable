import { Component, Input, OnInit } from '@angular/core';
import { Artist } from '../../models/artist';
import { ArtistService } from '../../services/artist/artist.service';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../services/auth/auth.service';
import { Observable } from 'rxjs';
import { DetailsDirective } from '../../directives/details/details.directive';
import { BlobStorageService } from '../../services/blob-storage/blob-storage.service';

@Component({
  selector: 'app-artist-details',
  standalone: false,
  
  templateUrl: './artist-details.component.html',
  styleUrl: './artist-details.component.scss'
})
export class ArtistDetailsComponent extends DetailsDirective<Artist> {
  constructor(
    private artistService: ArtistService,
    authService: AuthService,
    blobStorageService: BlobStorageService,
    route: ActivatedRoute,
    router: Router) 
  { 
    super(authService, blobStorageService, route, router)
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

  updateImage(url: string) {
    if(this.artist)
      this.artist.imageUrl = url
    this.onChangeDetected
  }
}
