import { Component, Input } from '@angular/core';
import { Event } from '../../models/event';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth/auth.service';
import { RoleNavigationService } from '../../services/role-navigation/role-navigation.service';

@Component({
  selector: 'app-item-events',
  standalone: false,
  templateUrl: './item-events.component.html',
  styleUrl: './item-events.component.scss'
})
export class ItemEventsComponent {
  @Input() events: Event[] = [];

  constructor(protected authService: AuthService, private router: Router, private roleNavigationService: RoleNavigationService) { }

  onViewDetails(event: Event) {
    this.roleNavigationService.navigateToResource(event, 'event');
  }

  onPurchase(event: Event) {
    this.router.navigate(['event/checkout', event.id])
  }
}
