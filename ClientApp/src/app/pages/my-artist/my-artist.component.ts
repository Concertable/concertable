import { Component } from '@angular/core';
import { Artist } from '../../models/artist';
import { ArtistService } from '../../services/artist/artist.service';
import { ActivatedRoute, Route, Router } from '@angular/router';

@Component({
  selector: 'app-my-artist',
  standalone: false,
  
  templateUrl: './my-artist.component.html',
  styleUrl: './my-artist.component.scss'
})
export class MyArtistComponent {
  protected artist?: Artist;
  protected editMode: boolean = false;

  constructor(private artistService: ArtistService, private router: Router, private route: ActivatedRoute) { }

  onEditModeChange(newValue: boolean) {
    this.editMode = newValue;
  }

  ngOnInit(): void {
    this.artistService.getDetailsForCurrentUser().subscribe((artist) => {
      this.artist = artist
    });
  }

  onCreateClick() {
    this.router.navigate(['create'], {
      relativeTo: this.route
    });
  }
}
