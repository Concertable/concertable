import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { HeaderService } from '../../services/header/header.service';
import { Header } from '../../models/header';
import { Observable } from 'rxjs';
import { ArtistHeader } from '../../models/artist-header';
import { VenueHeader } from '../../models/venue-header';
import { EventHeader } from '../../models/event-header';
import { HeaderType } from '../../models/header-type';

@Component({
  selector: 'app-search-results',
  standalone: false,
  templateUrl: './search-results.component.html',
  styleUrl: './search-results.component.scss'
})
export class SearchResultsComponent {
  @Input() headers: Header[] = [];
  @Input() headerType?: HeaderType;

  get artistHeaders(): ArtistHeader[] {
    return this.headers as ArtistHeader[];
  }

  get venueHeaders(): VenueHeader[] {
    return this.headers as VenueHeader[]; 
  }

  get eventHeaders(): EventHeader[] {
    return this.headers as EventHeader[];
  }
}
