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
  coordinates?: google.maps.LatLngLiteral;

  paginatedHeaders?: Pagination<Header>
  headers: Header[] = [];
  searchParams: SearchParams = {
    pageNumber: 1,
    pageSize: 10
  };

  constructor(
    private headerService: HeaderService, 
    private router: Router,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    const params = this.route.snapshot.queryParams;
    if (this.headerType && Object.keys(params).length > 0) {
        this.searchParams = {
            ...this.searchParams,
            ...params,
            date: params['date'] ? new Date(params['date']) : undefined,
            genreIds: params['genreIds']
              ? params['genreIds'].split(',').map((id: string) => Number(id))
              : undefined
          };
        this.handleSearch();
    };
}

  private headerMethods: Record<HeaderType, (searchParams: SearchParams) => Observable<Pagination<Header>>> = {
      venue: (searchParams) => this.headerService.getVenueHeaders(searchParams),
      artist: (searchParams) => this.headerService.getArtistHeaders(searchParams),
      event: (searchParams) => this.headerService.getEventHeaders(searchParams),
  };

  handleSearch() {
    if (this.headerType) {
      if(!this.searchParams.latitude || !this.searchParams.longitude)
        this.searchParams.radiusKm = undefined;
      const params = this.headerService.setParams(this.searchParams);
      const queryParams = Object.fromEntries(new URLSearchParams(params.toString()));
      this.router.navigate([], {
        relativeTo: this.route,
        queryParams: queryParams
      });
      this.headerMethods[this.headerType](this.searchParams).subscribe(p => {
        console.log(p);
        this.headers = p.data;
        this.paginatedHeaders = p
      }
      );
    }
  }
  
  changeHeaderType(headerType?: HeaderType) {
    this.headerType = headerType;
    this.headers = [];
  }

  updateSearchParams(updatedParams: Partial<SearchParams>) {
    Object.entries(updatedParams).forEach(([key, value]) => {
      if (value !== undefined) {
        (this.searchParams as any)[key] = value;
      }
    });
    console.log("Updated SearchParams:", this.searchParams);
  }
  

  onPageChange(event: { pageIndex: number; pageSize: number; }) {
    this.searchParams.pageNumber = event.pageIndex;
    this.searchParams.pageSize = event.pageSize
    this.handleSearch();
  }

}
