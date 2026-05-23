import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Observable, throwError } from 'rxjs';
import { ArtistService } from '../../services/artist/artist.service';
import { Artist } from '../../models/artist';

@Injectable({
  providedIn: 'root'
})
export class ArtistDetailsResolver implements Resolve<Artist> {

  constructor(private artistService: ArtistService) {}

  resolve(route: ActivatedRouteSnapshot): Observable<Artist> {
    const id = route.paramMap.get('id');

    if (!id) {
      return throwError(() => new Error("No artist ID provided"));
    }

    return this.artistService.getDetailsById(Number(id)); 
  }
}
