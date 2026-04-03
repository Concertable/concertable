import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Observable, throwError } from 'rxjs';
import { Artist } from '../../models/artist';
import { ArtistService } from '../../services/artist/artist.service';

@Injectable({
  providedIn: 'root'
})
export class MyArtistResolver implements Resolve<Artist> {

  constructor(private artistService: ArtistService) {}

  resolve(): Observable<Artist> {
    return this.artistService.getDetailsForCurrentUser();
  }
}
