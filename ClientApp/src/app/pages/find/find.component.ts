import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { HeaderService } from '../../services/header/header.service';
import { Header } from '../../models/header';
import { Pagination } from '../../models/pagination';
import { HeaderType } from '../../models/header-type';
import { SearchParams } from '../../models/search-params';
import { FindDirective } from '../../directives/find/find.directive';
import { SearchParamsSerializerServiceService } from '../../services/search-params-serializer/search-params-serializer-service.service';
import { EventHeader } from '../../models/event-header';
import { EventService } from '../../services/event/event.service';

@Component({
  selector: 'app-find',
  templateUrl: './find.component.html',
  standalone: false,
  styleUrl: './find.component.scss'
})
export class FindComponent extends FindDirective<Header> implements OnInit {
  
  coordinates?: google.maps.LatLngLiteral;
  headers: Header[] = [];

  constructor(
    searchParamsSerializerService: SearchParamsSerializerServiceService,
    router: Router,
    route: ActivatedRoute,
    private headerService: HeaderService,
    private eventService: EventService
  ) {
    super(searchParamsSerializerService, router, route);
  }

  ngOnInit(): void {
    // Initialize header type from route data
    this.searchParams.headerType = this.route.snapshot.data['headerType'];
    this.searchParamsSerializerService.defaultHeaderType = this.searchParams.headerType;

    // On URL change, deserialize search parameters and load data
    this.route.queryParams.subscribe((params: Params) => {
      if (Object.keys(params).length > 0) {
        console.log("?",params);
        this.searchParams = this.searchParamsSerializerService.deserialize(params);
        this.loadPage();
      }
    });
  }

  override changeHeaderType(headerType?: HeaderType) {
    super.changeHeaderType(headerType);
    this.headers = []; // Clear results when type changes
  }

  handleSearch() {
    console.log("test",this.searchParams)
    // Remove radius filter if no location specified
    if (this.searchParams?.headerType)
      if (!this.searchParams.latitude || !this.searchParams.longitude)
        this.searchParams.radiusKm = undefined;

    const params = this.searchParamsSerializerService.serialize(this.searchParams);

    // Update URL with new query params
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: params
    });
  }

  override loadPage(): void {
    console.log("etd")
    // Fetch search results based on current parameters
    this.headerService.get(this.searchParams).subscribe((p) => {
      this.paginatedData = p;
      this.headers = p.data;
    });
  }

  // DEV: Adds a test event manually into stream
  triggerNewSubscription() {
    const newEvent: EventHeader = {
      id: 999,
      name: '🔥 Injected Event',
      imageUrl: 'https://via.placeholder.com/200x200?text=🔥+Injected',
      county: 'Test County',
      town: 'Test Town',
      latitude: 51.5,
      longitude: -0.12,
      rating: 5,
      startDate: new Date(),
      endDate: new Date(new Date().getTime() + 2 * 60 * 60 * 1000),
    };

    this.eventService.addFakeEvent(newEvent);
  }
}
