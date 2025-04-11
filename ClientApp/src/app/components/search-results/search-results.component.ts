import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { HeaderService } from '../../services/header/header.service';
import { Header } from '../../models/header';
import { Observable } from 'rxjs';
import { ArtistHeader } from '../../models/artist-header';
import { VenueHeader } from '../../models/venue-header';
import { EventHeader } from '../../models/event-header';
import { HeaderType } from '../../models/header-type';
import { Pagination } from '../../models/pagination';
import { PageEvent } from '@angular/material/paginator';
import { PaginationParams } from '../../models/pagination-params';

@Component({
  selector: 'app-search-results',
  standalone: false,
  templateUrl: './search-results.component.html',
  styleUrl: './search-results.component.scss'
})
export class SearchResultsComponent {
  @Input() paginatedHeaders?: Pagination<Header>;
  @Input() headerType?: HeaderType;
  @Output() pageChange = new EventEmitter<PaginationParams>

  get artistHeaders(): ArtistHeader[] {
    return this.paginatedHeaders?.data as ArtistHeader[];
  }

  get venueHeaders(): VenueHeader[] {
    return this.paginatedHeaders?.data as VenueHeader[]; 
  }

  get eventHeaders(): EventHeader[] {
    return this.paginatedHeaders?.data as EventHeader[];
  }

  onPageChange(params: PaginationParams) {
    this.pageChange.emit(params);
  }
}
