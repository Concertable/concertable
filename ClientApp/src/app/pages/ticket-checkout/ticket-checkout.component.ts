import { Component, OnInit } from '@angular/core';
import { StripeService } from '../../services/stripe/stripe.service';
import { TicketService } from '../../services/ticket/ticket.service';

@Component({
  selector: 'app-ticket-checkout',
  standalone: false,
  templateUrl: './ticket-checkout.component.html',
  styleUrls: ['./ticket-checkout.component.css']
})
export class TicketCheckoutComponent implements OnInit {
  email: string = '';
  eventId: number = 1;
  message: string = '';
  isProcessing: boolean = false;

  constructor(private stripeService: StripeService, private ticketService: TicketService) {}

  async ngOnInit() {
    try {
      await this.stripeService.createCardElements();
    } catch (error) {
      this.message = 'Failed to initialize payment fields.';
      console.error(error);
    }
  }

  async purchaseTicket() {
    this.isProcessing = true;
    this.message = '';

    try {
      const paymentMethodId = await this.stripeService.createPaymentMethod(this.email);
      this.ticketService.purchase(paymentMethodId, this.eventId).subscribe(
        (response) => {
          if (response.success) {
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
    } catch (error: any) {
      this.message = `Payment error: ${error.message}`;
      console.error(error);
    } finally {
      this.isProcessing = false;
    }
  }
}
