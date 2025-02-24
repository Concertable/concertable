import { Directive, OnInit } from '@angular/core';
import { ListingApplication } from '../models/listing-application';
import { Event } from '../models/event';
import { ActivatedRoute } from '@angular/router';
import { StripeService } from '../services/stripe/stripe.service';
import { Purchase } from '../models/purchase';
import { Observable } from 'rxjs';

@Directive({
  selector: '[appCheckout]',
  standalone: false
})
export abstract class CheckoutDirective<T extends Event | ListingApplication> implements OnInit {
  checkoutEntity?: T
  message: string = '';
  isProcessing: boolean = false; 
  entityType?: string;

  constructor(private route: ActivatedRoute, private stripeService: StripeService) { }

  abstract setRouteData(data: any): void;

  abstract checkout(paymentMethodId: string, checkoutEntityId: number) : Observable<Purchase>

  abstract postCheckoutAction(): void

  async updatePaymentMethodId(paymentMethodId: string) {
    await this.completeCheckout(paymentMethodId)
  }

  async completeCheckout(paymentMethodId: string) {
    this.isProcessing = true;
    this.message = '';

    console.log("p",paymentMethodId)

    if(!paymentMethodId) {
      return;
    }

    try {
      if (this.checkoutEntity?.id) {
        this.checkout(paymentMethodId, this.checkoutEntity.id).subscribe(
          async (response) => {
            if (response.requiresAction && response.clientSecret) { //payment successful after 3d action complete
              try {
                await this.stripeService.confirmPayment(response.clientSecret);
                this.message = `Payment successful! ${this.entityType} purchased.`;
                this.postCheckoutAction();
              } catch (error: any) {
                this.message = `Payment authentication failed: ${error.message}`;
              }
            } else if (response.success) { //payment successful first time
              this.message = `Payment successful! ${this.entityType} purchased.`;
              this.postCheckoutAction();

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

  ngOnInit(): void {
    this.route.data.subscribe(data => {
      this.setRouteData(data);
    });
  }
  
}
