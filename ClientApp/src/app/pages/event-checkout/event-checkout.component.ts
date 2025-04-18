import { Component, OnInit } from '@angular/core';
import { StripeService } from '../../services/stripe/stripe.service';
import { TicketService } from '../../services/ticket/ticket.service';
import { Event } from '../../models/event';
import { EventStateService } from '../../services/event-state/event-state.service';
import { ActivatedRoute } from '@angular/router';
import { CheckoutDirective } from '../../directives/checkout/checkout.directive';
import { Observable } from 'rxjs';
import { TicketPurchase } from '../../models/ticket-purchase';
import { SignalRService } from '../../services/signalr/signalr.service';
import { BlobStorageService } from '../../services/blob-storage/blob-storage.service';
import { StripeToastService } from '../../services/toast/stripe-toast.service';

@Component({
  selector: 'app-event-checkout',
  standalone: false,
  templateUrl: './event-checkout.component.html',
  styleUrl: '../../shared/components/checkout.component.scss'
})
export class EventCheckoutComponent extends CheckoutDirective<Event> {
  override titleStyle: { [key: string]: string; } = { 'max-width': '1000px' };
  quantity: number = 1;
  override entityType = "ticket";

  constructor(
    route: ActivatedRoute, 
    stripeService: StripeService, 
    signalRService: SignalRService,
    blobStorageService: BlobStorageService,
    stripeToastService: StripeToastService,
    private ticketService: TicketService) {
      super(route, stripeService, signalRService, blobStorageService, stripeToastService);
  }

  override ngOnInit(): void {
    super.ngOnInit();
      if (this.event?.price && this.event?.artist?.name) 
        this.summaryItems = [
          { label: 'Cost per ticket', value: this.event.price, isCost: true },
          { label: 'Quantity', value: this.quantity, isCost: false },
          { label: 'Total', value: this.event.price * this.quantity, isCost: true }
        ];
      console.log(this.summaryItems);
      this.signalRService.ticketPurchased$.subscribe(t => {
        this.stripeToastService.showTicketPurchase(t.ticketId);
      })
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
      return this.ticketService.purchase(paymentMethodId, eventId, this.quantity);
  }

  postCheckoutAction(): void {
      
  }

  updateQuantity(quantity: number) {
    this.quantity = quantity;
  }
}
