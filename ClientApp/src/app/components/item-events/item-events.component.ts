import { AfterViewInit, Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { Event } from '../../models/event';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth/auth.service';
import { RoleNavigationService } from '../../services/role-navigation/role-navigation.service';
import { Observable } from 'rxjs';
import { TicketService } from '../../services/ticket/ticket.service';

@Component({
  selector: 'app-item-events',
  standalone: false,
  templateUrl: './item-events.component.html',
  styleUrl: './item-events.component.scss'
})
export class ItemEventsComponent implements OnChanges {
  @Input() events: Event[] = [];
  canPurchase$s: Observable<boolean>[] = [];

  constructor(
    protected authService: AuthService, 
    private router: Router, 
    private roleNavigationService: RoleNavigationService,
    private ticketService: TicketService
  ) { }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['events'] && this.events.length > 0) 
      this.canPurchase$s = this.events.map(event => this.ticketService.canPurchase(event.id));
  }

  onViewDetails(event: Event) {
    this.roleNavigationService.navigateToResource(event, 'event');
  }

  onPurchase(event: Event) {
    this.router.navigate(['event/checkout', event.id])
  }
}
