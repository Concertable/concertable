import { Component } from '@angular/core';
import { Artist } from '../../models/artist';
import { ArtistService } from '../../services/artist/artist.service';
import { ActivatedRoute, Route, Router } from '@angular/router';
import { ConfigDirective } from '../../directives/config/config.directive';
import { Observable } from 'rxjs';
import { ArtistToastService } from '../../services/toast/artist/artist-toast.service';

@Component({
  selector: 'app-my-artist',
  standalone: false,
  
  templateUrl: './my-artist.component.html',
  styleUrl: './my-artist.component.scss'
})
export class MyArtistComponent extends ConfigDirective<Artist> {
  constructor(
    route: ActivatedRoute, 
    private artistService: ArtistService, 
    private artistToastService: ArtistToastService,
    private router: Router) {
      super(route);
     }

  get artist() : Artist | undefined {
    return this.item;
  }

  set artist(artist: Artist) {
    this.item = artist;
  }

  setDetails(data: any): void {
    this.artist = data['artist'];    
    console.log(this.artist);
  }

  update(artist: Artist): Observable<Artist> {
    return this.artistService.update(artist, this.image);
  }

  showUpdated(artist: Artist): void {
    this.artistToastService.showUpdated(artist.name);
  }
}
