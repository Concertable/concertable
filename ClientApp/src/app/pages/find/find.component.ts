import { Component, Input } from '@angular/core';
import { SearchType } from '../../models/search-type';
import { EventService } from '../../services/event/event.service';
import { HeaderService } from '../../services/header/header.service';
import { Observable } from 'rxjs';
import { Header } from '../../models/header';
import { ArtistHeader } from '../../models/artist-header';
import { VenueHeader } from '../../models/venue-header';
import { EventHeader } from '../../models/event-header';
import { SearchParams } from '../../models/search-params';

@Component({
  selector: 'app-find',
  standalone: false,
  
  templateUrl: './find.component.html',
  styleUrl: './find.component.scss'
})
export class FindComponent {
    @Input() searchType?: SearchType;
    headers: Header[] = [];
  
    constructor(private headerService: HeaderService) { }

    private headerMethods: Record<SearchType, (searchParams: SearchParams) => Observable<Header[]>> = {
        venue: (searchParams) => this.headerService.getVenueHeaders(searchParams),
        artist: (searchParams) => this.headerService.getArtistHeaders(searchParams),
        event: (searchParams) => this.headerService.getEventHeaders(searchParams),
    };
  
    handleSearch(searchParams: SearchParams) {
      if (this.searchType) {
        this.headerMethods[this.searchType](searchParams).subscribe(h => {
          console.log('API Response:', h); // Debugging API response
          this.headers = h;
          console.log('Updated Headers:', this.headers);
          console.log(h[0].type === 'venue');
        }
        );
      }

    }
    
}
