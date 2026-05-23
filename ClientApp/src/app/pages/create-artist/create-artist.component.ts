import { Component } from '@angular/core';
import { Artist } from '../../models/artist';
import { CreateItemDirective } from '../../directives/create-item/create-item.directive';
import { EMPTY, Observable } from 'rxjs';
import { ArtistService } from '../../services/artist/artist.service';
import { ArtistToastService } from '../../services/toast/artist/artist-toast.service';
import { ActivatedRoute, Router } from '@angular/router';
import { validateObject } from '../../shared/validators/object-validator';

@Component({
  selector: 'app-create-artist',
  standalone: false,
  
  templateUrl: './create-artist.component.html',
  styleUrl: './create-artist.component.scss'
})
export class CreateArtistComponent extends CreateItemDirective<Artist> {
  constructor(
      private artistService: ArtistService, 
      private artistToastService: ArtistToastService,
      router: Router,
      route: ActivatedRoute
    ) {
      super(router, route);
    }

  get artist(): Artist | undefined {
      return this.item;
    }
  
    set artist(value: Artist | undefined) {
      this.item = value;
    }

  createDefaultItem(): void {
    this.artist =  {
      id: 0,  
      type: 'artist',
      name: "",
      about: "",
      imageUrl: "", 
      genres: [],
      county: "",
      town: "",
      email: '',
      rating: 0
    };
  }

  create(artist: Artist): Observable<Artist> {
    if (!this.image) {
          this.artistToastService.showError("Image is required", "Validation Error");
          return EMPTY;
        }
        const errors = validateObject(artist, ['id', 'email', 'county', 'town', 'imageUrl']);
        if (errors.length) {
          this.artistToastService.showErrors(errors, "Validation Error");
          return EMPTY;
        }
        return this.artistService.create(artist, this.image);
  }
  
  showCreated(artist: Artist): void {
    this.artistToastService.showCreated(artist.name);
  }
}
