import { Component, Input } from '@angular/core';
import { DashboardDirective } from '../../directives/dashboard/dashboard.directive';
import { Venue } from '../../models/venue';
import { EventViewType } from '../../models/event-view-type';

@Component({
  selector: 'app-venue',
  standalone: false,
  
  templateUrl: './venue-dashboard.component.html',
  styleUrl: './venue-dashboard.component.scss'
})
export class VenueDashboardComponent extends DashboardDirective<Venue> {
  get venue(): Venue | undefined {
      return this.item;
  }
    
  @Input()
  set venue(venue: Venue | undefined) {
    this.item = venue;
  }

  setDetails(data: any): void {
    this.venue = data['venue'];
    console.log(this.venue);
  }
}
