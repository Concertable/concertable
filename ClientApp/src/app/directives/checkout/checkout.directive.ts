import { Directive, OnDestroy, OnInit } from '@angular/core';
import { ListingApplication } from '../../models/listing-application';
import { Event } from '../../models/event';
import { ActivatedRoute } from '@angular/router';
import { StripeService } from '../../services/stripe/stripe.service';
import { Purchase } from '../../models/purchase';
import { Observable, Subscription } from 'rxjs';
import { SignalRService } from '../../services/signalr/signalr.service';
import { BlobStorageService } from '../../services/blob-storage/blob-storage.service';
import { PaymentSummaryItem } from '../../models/payment-summary-item';
import { StripeToastService } from '../../services/toast/stripe-toast.service';

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
  summaryItems: PaymentSummaryItem[] = [];
  titleStyle: { [key: string]: string } = {};

  constructor(
    private route: ActivatedRoute, 
    private stripeService: StripeService,
    protected signalRService: SignalRService,
    protected blobStorageService: BlobStorageService,
    protected stripeToastService: StripeToastService
  ) { }

  ngOnInit(): void {
    this.route.data.subscribe(data => {
      this.setRouteData(data);
    });
  }

  abstract setRouteData(data: any): void;

  abstract checkout(paymentMethodId: string, checkoutEntityId: number) : Observable<Purchase>

  async updatePaymentMethodId(paymentMethodId: string) {
    await this.completeCheckout(paymentMethodId)
  }

  async completeCheckout(paymentMethodId: string) {
    this.isProcessing = true;

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
                this.stripeToastService.showSuccess(`${this.entityType} purchased`, "Payment Successful")
              } catch (error: any) {
                this.stripeToastService.showError(error.message, "Payment authentication failed")
              }
            } else if (response.success) { //payment successful first time
              this.stripeToastService.showSuccess(`${this.entityType} purchased`, "Payment Successful")
            } else {
              this.stripeToastService.showError("Error processing payment", "Payment Failed")
            }
          },
          (error) => {
            this.stripeToastService.showError("Error processing payment", "Payment Failed")
            console.error(error);
          }
        );
      }
    } catch (error: any) {
      this.stripeToastService.showSuccess("Error processing payment", "Payment Failed")
      console.error(error);
    } finally {
      this.isProcessing = false;
    }
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }
  
}
