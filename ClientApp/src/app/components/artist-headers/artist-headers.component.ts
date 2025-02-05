import { Component, Input } from '@angular/core';
import { ArtistHeader } from '../../models/artist-header';

@Component({
  selector: 'app-artist-headers',
  standalone: false,
  
  templateUrl: './artist-headers.component.html',
  styleUrl: './artist-headers.component.scss'
})
export class ArtistHeadersComponent {
  @Input() headers: ArtistHeader[] = []
}
