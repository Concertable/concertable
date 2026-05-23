import { Component } from '@angular/core';
import { SearchParams } from '../../models/search-params';
import { HeaderType } from '../../models/header-type';

@Component({
  selector: 'app-venue-find',
  standalone: false,
  
  templateUrl: './venue-find.component.html',
  styleUrl: './venue-find.component.scss'
})
export class VenueFindComponent {
  headerType: HeaderType = 'artist';
}
