import { Component, Input, OnInit } from '@angular/core';
import { SearchType } from '../../models/search-type';
import { EventService } from '../../services/event/event.service';
import { HeaderService } from '../../services/header/header.service';
import { Observable } from 'rxjs';
import { Header } from '../../models/header';
import { ArtistHeader } from '../../models/artist-header';
import { VenueHeader } from '../../models/venue-header';
import { EventHeader } from '../../models/event-header';
import { SearchParams } from '../../models/search-params';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-find',
  standalone: false,
  
  templateUrl: './find.component.html',
  styleUrl: './find.component.scss'
})
export class FindComponent implements OnInit {
  @Input() searchType?: SearchType;
  headers: Header[] = [];

  constructor(
    private headerService: HeaderService, 
    private router: Router,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      if (this.searchType && Object.keys(params).length > 0)
        this.handleSearch(params as SearchParams);
    })
  }

  private headerMethods: Record<SearchType, (searchParams: SearchParams) => Observable<Header[]>> = {
      venue: (searchParams) => this.headerService.getVenueHeaders(searchParams),
      artist: (searchParams) => this.headerService.getArtistHeaders(searchParams),
      event: (searchParams) => this.headerService.getEventHeaders(searchParams),
  };

  handleSearch(searchParams: SearchParams) {
    if (this.searchType) {
      this.router.navigate([], {
        relativeTo: this.route,
        queryParams: searchParams,
      });
      this.headerMethods[this.searchType](searchParams).subscribe(h => {
        this.headers = h;
      }
      );
    }
  }
    
}
