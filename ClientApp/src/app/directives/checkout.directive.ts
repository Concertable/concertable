import { Directive, OnDestroy, OnInit } from '@angular/core';
import { ListingApplication } from '../models/listing-application';
import { Event } from '../models/event';
import { ActivatedRoute } from '@angular/router';
import { StripeService } from '../services/stripe/stripe.service';
import { Purchase } from '../models/purchase';
import { Observable, Subscription } from 'rxjs';
import { SignalRService } from '../services/signalr/signalr.service';

@Directive({
  selector: '[appCheckout]',
  standalone: false
})
export abstract class CheckoutDirective<T extends Event | ListingApplication> implements OnInit, OnDestroy {
  checkoutEntity?: T
  message: string = '';
  isProcessing: boolean = false; 
  entityType?: string;
  subscriptions: Subscription[] = [];

  constructor(
    private route: ActivatedRoute, 
    private stripeService: StripeService,
    protected signalRService: SignalRService  
  ) { }

  abstract setRouteData(data: any): void;

  abstract checkout(paymentMethodId: string, checkoutEntityId: number) : Observable<Purchase>

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
      console.log("test",this.checkoutEntity);
      if (this.checkoutEntity?.id) {
        this.checkout(paymentMethodId, this.checkoutEntity.id).subscribe(
          async (response) => {
            if (response.requiresAction && response.clientSecret) { //payment successful after 3d action complete
              try {
                await this.stripeService.confirmPayment(response.clientSecret);
                this.message = `Payment successful! ${this.entityType} purchased.`;
              } catch (error: any) {
                this.message = `Payment authentication failed: ${error.message}`;
              }
            } else if (response.success) { //payment successful first time
              this.message = `Payment successful! ${this.entityType} purchased.`;

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

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }
  
}
