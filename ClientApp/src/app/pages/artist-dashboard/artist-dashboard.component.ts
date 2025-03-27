import { Component, Input } from '@angular/core';
import { Artist } from '../../models/artist';
import { DashboardDirective } from '../../directives/dashboard.directive';

@Component({
  selector: 'app-artist-dashboard',
  standalone: false,
  
  templateUrl: './artist-dashboard.component.html',
  styleUrl: './artist-dashboard.component.scss'
})
export class ArtistDashboardComponent extends DashboardDirective<Artist> {
  get artist(): Artist | undefined {
        return this.item;
    }
      
    @Input()
    set artist(artist: Artist | undefined) {
      this.item = artist;
    }

  setDetails(data: any): void {
    this.artist = data['artist'];   
    console.log(data);
  }
}
