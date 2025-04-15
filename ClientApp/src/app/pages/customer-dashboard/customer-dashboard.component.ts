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

@Component({
  selector: 'app-customer-dashboard',
  standalone: false,
  templateUrl: './customer-dashboard.component.html',
  styleUrl: './customer-dashboard.component.scss'
})
export class CustomerDashboardComponent extends FindDirective<EventHeader> implements OnInit, OnDestroy {
  triggerNewSubscription() {
    // Create a new EventHeader to simulate a new event being posted
    const newEventHeader: EventHeader = {
      id: Math.floor(Math.random() * 1000),  // Generate a random ID for the new event
      name: 'New Event ' + new Date().toISOString(),
      imageUrl: 'https://via.placeholder.com/200x200?text=New+Event',
      county: 'New County',
      town: 'New Town',
      latitude: 34.0194,
      longitude: -118.4912,
      rating: Math.random() * 5,
      startDate: new Date(),
      endDate: new Date(new Date().getTime() + 4 * 60 * 60 * 1000)  // 4 hours later
    };
  
    this.eventService.addFakeEvent(newEventHeader);
  }
  
  private eventSubscription!: Subscription;

  recentEventHeaders: EventHeader[] = [];
  eventHeaders: EventHeader[] = [];
  venueHeaders: VenueHeader[] = [];
  artistHeaders: ArtistHeader[] = []
  
  constructor(
    searchParamsSerializerService: SearchParamsSerializerServiceService,
    router: Router,
    route: ActivatedRoute,
    private eventService: EventService
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
    this.subscriptions.push(this.eventService.recentEventHeaders$.subscribe(header => {
      this.recentEventHeaders.unshift(header);
      if (this.recentEventHeaders.length > 10) {
        this.recentEventHeaders.pop();  
      }
      console.log(this.recentEventHeaders);
    }));
  }
}

