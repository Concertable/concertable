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
    this.router.navigate(['find/event'], { 
      queryParams: { id: event.id } 
    });
  }

  onPurchase(event: Event) {
    
  }
}
