import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ListingApplicationService } from '../../services/listing-application/listing-application.service';
import { ListingApplication } from '../../models/listing-application';
import { Artist } from '../../models/artist';
import { EventService } from '../../services/event/event.service';
import { EventToastService } from '../../services/toast/event/event-toast.service';

@Component({
  selector: 'app-listing-applications',
  standalone: false,
  templateUrl: './listing-applications.component.html',
  styleUrl: './listing-applications.component.scss'
})
export class ListingApplicationsComponent implements OnInit {
  applications: ListingApplication[] = [];

  constructor(
    private route: ActivatedRoute, 
    private listingApplicationService: ListingApplicationService,
    private eventService: EventService,
    private router: Router,
    private eventToastService: EventToastService) { }

  get artists(): Artist[] {
    return this.applications.map(app => app.artist);
  }
  

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      const listingId = params['listingId']
      if(listingId)
        this.listingApplicationService.getAllForListingId(listingId).subscribe(a => this.applications = a);
    });
  }

  acceptApplication(application: ListingApplication) {
    this.router.navigate(['application/checkout', application?.id]);
    // this.eventService.createFromApplicationId(application.id).subscribe(() => {
    //   this.router.navigate([`../events/event`], {
    //     relativeTo: this.route,
    //     queryParams: { id: 1 }
    //   })
    //   this.eventToastService.showAutoCreated()
    // });
  }
}


