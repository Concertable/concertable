import { Component, Input } from '@angular/core';
import { Artist } from '../../models/artist';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-artist-summary',
  standalone: false,
  templateUrl: './artist-summary.component.html',
  styleUrl: './artist-summary.component.scss'
})
export class ArtistSummaryComponent {
  @Input() artist!: Artist;

  constructor(private router: Router, private route: ActivatedRoute) { }

  onDetailsClick() {
    this.router.navigate(['../artist'], { 
      relativeTo: this.route, 
      queryParams: { id: this.artist.id } 
    });
  }
}
