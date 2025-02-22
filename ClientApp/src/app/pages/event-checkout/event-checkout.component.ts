import { Component, OnInit } from '@angular/core';
import { StripeService } from '../../services/stripe/stripe.service';
import { TicketService } from '../../services/ticket/ticket.service';
import { Event } from '../../models/event';
import { EventStateService } from '../../services/event-state/event-state.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-event-checkout',
  standalone: false,
  templateUrl: './event-checkout.component.html',
  styleUrls: ['./event-checkout.component.scss']
})
export class EventCheckoutComponent implements OnInit {
  event?: Event;
  email: string = '';
  message: string = '';
  isProcessing: boolean = false; 

  constructor(
    private route: ActivatedRoute, 
    private stripeService: StripeService, 
    private ticketService: TicketService) {}
  
  ngOnInit() {
    this.route.data.subscribe(data => {
      this.event = data['event'];
    });
  }

  async updatePaymentMethodId(paymentMethodId: string) {
    await this.purchaseTicket(paymentMethodId)
  }

  async purchaseTicket(paymentMethodId: string) {
    this.isProcessing = true;
    this.message = '';

    if(!paymentMethodId) {
      return;
    }

    try {
      if (this.event?.id) {
        this.ticketService.purchase(this.event.id, paymentMethodId).subscribe(
          async (response) => {
            if (response.requiresAction && response.clientSecret) {
              try {
                await this.stripeService.confirmPayment(response.clientSecret, this.email);
                this.message = 'Payment successful! Ticket purchased.';
              } catch (error: any) {
                this.message = `Payment authentication failed: ${error.message}`;
              }
            } else if (response.success) {
              this.message = 'Payment successful! Ticket purchased.';
            } else {
              this.message = 'Payment failed.';
            }
          },
          (error) => {
            this.message = 'Error processing payment.';
            console.error(error);
          }
        );
      }
    } catch (error: any) {
      this.message = `Payment error: ${error.message}`;
      console.error(error);
    } finally {
      this.isProcessing = false;
    }
  }
  
  
}
