import { Component } from '@angular/core';
import { SearchParams } from '../../models/search-params';
import { HeaderType } from '../../models/header-type';

@Component({
  selector: 'app-artist-find',
  standalone: false,
  
  templateUrl: './artist-find.component.html',
  styleUrl: './artist-find.component.scss'
})
export class ArtistFindComponent {
  headerType: HeaderType = 'venue';
}
