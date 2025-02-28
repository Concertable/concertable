import { Component, OnDestroy, OnInit } from '@angular/core';
import { SearchParams } from '../../models/search-params';
import { HeaderType } from '../../models/header-type';
import { FindDirective } from '../../directives/find.directive';
import { EventService } from '../../services/event/event.service';
import { Event } from '../../models/event';
import { SearchParamsSerializerServiceService } from '../../services/search-params-serializer/search-params-serializer-service.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-customer-dashboard',
  standalone: false,
  templateUrl: './customer-dashboard.component.html',
  styleUrl: './customer-dashboard.component.scss'
})
export class CustomerDashboardComponent extends FindDirective implements OnInit, OnDestroy {
  private eventSubscription!: Subscription;
  
  constructor(
    searchParamsSerializerService: SearchParamsSerializerServiceService,
    router: Router,
    route: ActivatedRoute,
    private eventService: EventService
  ) {
    super(searchParamsSerializerService, router, route);
  }
  handleSearch() {
    const params = this.searchParamsSerializerService.serialize(this.searchParams);
    this.router.navigate(['/find'],
      { queryParams: params }
    )
  }

  ngOnInit(): void {
    this.eventSubscription = this.eventService.recentEvents$.subscribe(e => {
      console.log(e);
    });
  }

  ngOnDestroy(): void {
    this.eventSubscription?.unsubscribe();
  }
  
}

