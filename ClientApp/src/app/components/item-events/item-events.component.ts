import { Component, Input } from '@angular/core';
import { Event } from '../../models/event';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth/auth.service';

@Component({
  selector: 'app-item-events',
  standalone: false,
  templateUrl: './item-events.component.html',
  styleUrl: './item-events.component.scss'
})
export class ItemEventsComponent {
  @Input() events: Event[] = [];

  constructor(protected authService: AuthService, private router: Router) { }

  onViewDetails(event: Event) {
    this.authService.currentUser$.subscribe(user => {
      console.log(this.authService.isRole("VenueManager"))
      console.log(event.venue.email == user?.email)
      if(this.authService.isRole("VenueManager"))
        if(event.venue.email == user?.email)
          this.router.navigate(['venue/my/events/event', event.id]);
      else if(this.authService.isRole("ArtistManager"))
        if(event.artist.email == user?.email)
          this.router.navigate(['artist/my/events/event', event.id]);
      else 
        this.router.navigate(['find/event', event.id]);
    })
  }

  onPurchase(event: Event) {
    
  }
}
