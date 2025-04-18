import { Component } from '@angular/core';
import { ListingApplicationService } from '../../services/listing-application/listing-application.service';
import { ListingApplication } from '../../models/listing-application';
import { ArtistListingApplication } from '../../models/artist-listing-application';
import { Router } from '@angular/router';

@Component({
  selector: 'app-artist-listing-applications',
  standalone: false,
  templateUrl: './artist-listing-applications.component.html',
  styleUrl: './artist-listing-applications.component.scss'
})
export class ArtistListingApplicationsComponent {
  pendingApplications: ArtistListingApplication[] = [];
  deniedApplications: ArtistListingApplication[] =[];

  constructor(private listingApplicationService: ListingApplicationService, private router: Router) {}

  ngOnInit(): void {
    this.listingApplicationService.getPendingForArtist().subscribe(apps => {
      this.pendingApplications = apps;
    });
    this.listingApplicationService.getRecentDeniedForArtist().subscribe(apps => {
      this.deniedApplications = apps;
    });
  }

  onViewDetails(application: ArtistListingApplication) {
      this.router.navigate([`/find/venue`, application.listingWithVenue.venue.id]);
  }
}