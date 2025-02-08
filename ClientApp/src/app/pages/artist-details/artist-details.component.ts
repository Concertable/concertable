import { Component, Input, OnInit } from '@angular/core';
import { Artist } from '../../models/artist';
import { ArtistService } from '../../services/artist/artist.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-artist-details',
  standalone: false,
  
  templateUrl: './artist-details.component.html',
  styleUrl: './artist-details.component.scss'
})
export class ArtistDetailsComponent implements OnInit{
  @Input() artist?: Artist;
  @Input() editMode?: boolean

  constructor(private artistService: ArtistService, private route: ActivatedRoute) { }

  ngOnInit() {
    if (!this.artist) {
      this.route.queryParams.subscribe(params => {
        const artistId = params['id'];
        if (artistId) {
          this.artistService.getDetailsById(artistId).subscribe(artist => this.artist=artist);
        }
      });
    }
  }
}
