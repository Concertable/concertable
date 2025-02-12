import { Component } from '@angular/core';
import { Artist } from '../../models/artist';

@Component({
  selector: 'app-create-artist',
  standalone: false,
  
  templateUrl: './create-artist.component.html',
  styleUrl: './create-artist.component.scss'
})
export class CreateArtistComponent {
  artist: Artist = {
  id: 0,  
  name: "",
  about: "",
  imageUrl: "", 
  genres: [],
  county: "",
  town: ""
  };
}
