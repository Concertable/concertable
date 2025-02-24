import { Component, OnInit } from '@angular/core';
import { StripeService } from '../../services/stripe/stripe.service';
import { TicketService } from '../../services/ticket/ticket.service';
import { Event } from '../../models/event';
import { EventStateService } from '../../services/event-state/event-state.service';
import { ActivatedRoute } from '@angular/router';
import { CheckoutDirective } from '../../directives/checkout.directive';
import { Observable } from 'rxjs';
import { TicketPurchase } from '../../models/ticket-purchase';

@Component({
  selector: 'app-event-checkout',
  standalone: false,
  templateUrl: './event-checkout.component.html',
  styleUrls: ['./event-checkout.component.scss']
})
export class EventCheckoutComponent extends CheckoutDirective<Event> {
  constructor(
    route: ActivatedRoute, 
    stripeService: StripeService, 
    private ticketService: TicketService) {
      super(route, stripeService);
    }

  get event(): Event | undefined {
    return this.checkoutEntity;
  }

  set event(value: Event | undefined) {
    this.checkoutEntity = value;
  }

  setRouteData(data: any): void {
    this.event = data['event'];
  }

  checkout(paymentMethodId: string, eventId: number): Observable<TicketPurchase> {
      return this.ticketService.purchase(paymentMethodId, eventId);
  }

  postCheckoutAction(): void {
      
  }
}
