import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { EventService } from '../../services/event/event.service';
import { HeaderService } from '../../services/header/header.service';
import { Observable } from 'rxjs';
import { Header } from '../../models/header';
import { ArtistHeader } from '../../models/artist-header';
import { VenueHeader } from '../../models/venue-header';
import { EventHeader } from '../../models/event-header';
import { SearchParams } from '../../models/search-params';
import { ActivatedRoute, Router } from '@angular/router';
import { HeaderType } from '../../models/header-type';
import { Pagination } from '../../models/pagination';

@Component({
  selector: 'app-find',
  standalone: false,
  
  templateUrl: './find.component.html',
  styleUrl: './find.component.scss'
})
export class FindComponent implements OnInit {
  @Input() headerType?: HeaderType;
  @Input() isCustomer?: boolean = false;
  headers: Header[] = [];

  constructor(
    private headerService: HeaderService, 
    private router: Router,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      if (this.headerType && Object.keys(params).length > 0)
        this.handleSearch(params as SearchParams);
    })
  }

  private headerMethods: Record<HeaderType, (searchParams: SearchParams) => Observable<Pagination<Header>>> = {
      venue: (searchParams) => this.headerService.getVenueHeaders(searchParams),
      artist: (searchParams) => this.headerService.getArtistHeaders(searchParams),
      event: (searchParams) => this.headerService.getEventHeaders(searchParams),
  };

  handleSearch(searchParams: SearchParams) {
    if (this.headerType) {
      this.router.navigate([], {
        relativeTo: this.route,
        queryParams: searchParams,
      });
      this.headerMethods[this.headerType](searchParams).subscribe(h => {
        console.log(h);
        this.headers = h.data;
      }
      );
    }
  }
  
  changeHeaderType(headerType?: HeaderType) {
    this.headerType = headerType;
    this.headers = [];
  }
  }
