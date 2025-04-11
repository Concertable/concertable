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
  eventHeaders: EventHeader[] = [
    {
      id: 1,
      name: 'Sunset Festival',
      imageUrl: 'rockers.jpg',
      county: 'Palm County',
      town: 'Beachville',
      latitude: 34.0194,
      longitude: -118.4912,
      rating: 4.8,
      startDate: new Date('2025-04-01T17:00:00'),
      endDate: new Date('2025-04-01T23:00:00')
    },
    {
      id: 2,
      name: 'Jazz Night',
      imageUrl: 'rockers.jpg',
      county: 'Metro County',
      town: 'Groovetown',
      latitude: 40.7128,
      longitude: -74.0060,
      rating: 4.5,
      startDate: new Date('2025-04-03T20:00:00'),
      endDate: new Date('2025-04-04T01:00:00')
    },
    {
      id: 3,
      name: 'Food Truck Fiesta',
      imageUrl: 'rockers.jpg',
      county: 'Taste County',
      town: 'Flavorburg',
      latitude: 37.7749,
      longitude: -122.4194,
      rating: 4.7,
      startDate: new Date('2025-04-05T12:00:00'),
      endDate: new Date('2025-04-05T18:00:00')
    },
    {
      id: 4,
      name: 'Rock in the Park',
      imageUrl: 'rockers.jpg',
      county: 'Sound County',
      town: 'AmpCity',
      latitude: 51.5074,
      longitude: -0.1278,
      rating: 4.9,
      startDate: new Date('2025-04-07T18:00:00'),
      endDate: new Date('2025-04-07T22:30:00')
    },
    {
      id: 5,
      name: 'Outdoor Movie Night',
      imageUrl: 'rockers.jpg',
      county: 'Cinema County',
      town: 'Reelville',
      latitude: 48.8566,
      longitude: 2.3522,
      rating: 4.3,
      startDate: new Date('2025-04-08T20:30:00'),
      endDate: new Date('2025-04-08T23:00:00')
    }
  ];

  recentEventHeaders: EventHeader[] = [];
  
  
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
    this.eventSubscription = this.eventService.recentEventHeaders$.subscribe(header => {
      this.recentEventHeaders.unshift(header);
      if (this.recentEventHeaders.length > 10) {
        this.recentEventHeaders.pop();  
      }
    });
  }
  

  ngOnDestroy(): void {
    this.eventSubscription?.unsubscribe();
  }
  
}

