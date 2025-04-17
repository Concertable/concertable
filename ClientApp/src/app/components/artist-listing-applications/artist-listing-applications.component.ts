import { Component } from '@angular/core';
import { ListingApplicationService } from '../../services/listing-application/listing-application.service';
import { ListingApplication } from '../../models/listing-application';
import { ArtistListingApplication } from '../../models/artist-listing-application';

@Component({
  selector: 'app-artist-listing-applications',
  standalone: false,
  templateUrl: './artist-listing-applications.component.html',
  styleUrl: './artist-listing-applications.component.scss'
})
export class ArtistListingApplicationsComponent {
  applications: ArtistListingApplication[] = [];

  constructor(private listingApplicationService: ListingApplicationService) {}

  ngOnInit(): void {
    this.listingApplicationService.getActiveApplicationsForArtist().subscribe(apps => {
      console.log("Returned apps:", apps); // ← confirm this
      this.applications = apps;
    });
    
  }
}