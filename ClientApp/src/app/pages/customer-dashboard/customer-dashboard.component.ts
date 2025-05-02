import { Component, OnDestroy, OnInit } from '@angular/core';
import { SearchParams } from '../../models/search-params';
import { HeaderType } from '../../models/header-type';
import { FindDirective } from '../../directives/find/find.directive';
import { EventService } from '../../services/event/event.service';
import { Event } from '../../models/event';
import { SearchParamsSerializerServiceService } from '../../services/search-params-serializer/search-params-serializer-service.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { EventHeader } from '../../models/event-header';
import { ArtistHeader } from '../../models/artist-header';
import { VenueHeader } from '../../models/venue-header';
import { RecommendedEventsService } from '../../services/recommended-events/recommended-events.service';

@Component({
  selector: 'app-customer-dashboard',
  standalone: false,
  templateUrl: './customer-dashboard.component.html',
  styleUrl: './customer-dashboard.component.scss'
})
export class CustomerDashboardComponent extends FindDirective<EventHeader> implements OnInit, OnDestroy {
  private eventSubscription!: Subscription;

  recommendedEventHeaders: EventHeader[] = [];
  eventHeaders: EventHeader[] = [];
  venueHeaders: VenueHeader[] = [];
  artistHeaders: ArtistHeader[] = []
  
  constructor(
    searchParamsSerializerService: SearchParamsSerializerServiceService,
    router: Router,
    route: ActivatedRoute,
    private recommendedEventsService: RecommendedEventsService
  ) {
    super(searchParamsSerializerService, router, route);
  }
  loadPage() {
    const params = this.searchParamsSerializerService.serialize(this.searchParams);
    this.router.navigate(['/find'],
      { queryParams: params }
    )
  }

  ngOnInit(): void {
    this.recommendedEventsService.init();

    this.subscriptions.push(
      this.recommendedEventsService.headers$.subscribe(header => {
        this.recommendedEventHeaders.unshift(header);
        if (this.recommendedEventHeaders.length > 10) 
          this.recommendedEventHeaders.pop();
      })
    );
  }
}

