import { Component } from '@angular/core';
import { CheckoutDirective } from '../../directives/checkout/checkout.directive';
import { ListingApplication } from '../../models/listing-application';
import { ActivatedRoute, Router } from '@angular/router';
import { Listing } from '../../models/listing';
import { EventService } from '../../services/event/event.service';
import { Observable } from 'rxjs';
import { Purchase } from '../../models/purchase';
import { StripeService } from '../../services/stripe/stripe.service';
import { ListingApplicationPurchase } from '../../models/listing-application-purchase';
import { SignalRService } from '../../services/signalr/signalr.service';
import { BlobStorageService } from '../../services/blob-storage/blob-storage.service';
import { StripeToastService } from '../../services/toast/stripe-toast.service';

@Component({
  selector: 'app-listing-application-checkout',
  standalone: false,
  templateUrl: './listing-application-checkout.component.html',
  styleUrl: '../../shared/components/checkout/checkout.component.scss'
})
export class ListingApplicationCheckoutComponent extends CheckoutDirective<ListingApplication> {
  override titleStyle: { [key: string]: string; } = { 'max-width': '1000px' };
  override entityType = 'ListingApplication'

  constructor(
    route: ActivatedRoute, 
    stripeService: StripeService, 
    signalRService: SignalRService,
    blobStorageService: BlobStorageService,
    stripeToastService: StripeToastService,
    private eventService: EventService,
    private router: Router
  ) {
    super(route, stripeService, signalRService, blobStorageService, stripeToastService);
  }

  get application(): ListingApplication | undefined {
      return this.checkoutEntity;
    }
  
  set application(value: ListingApplication | undefined) {
    this.checkoutEntity = value;
  }

  override ngOnInit(): void {
      super.ngOnInit();
      if (this.application?.artist) 
        this.summaryItems = [
          { label: "Artist you're paying to perform", value: this.application.artist.name },
          { label: "Amount", value: this.application.listing.pay, isCost: true },
          { label: "Total", value: this.application.listing.pay, isCost: true }
        ];
      console.log(this.summaryItems);
      this.signalRService.eventCreated$.subscribe(response => {
        var event = response?.event
        console.log("subscribed", response);
        if(event)
          this.router.navigateByUrl(`venue/my/events/event/${event.id}`);
      })
  }

  setRouteData(data: any): void {
    console.log(data);
    this.application = data['application'];
    console.log("a",this.application);
  }

  checkout(paymentMethodId: string, applicationId: number): Observable<ListingApplicationPurchase> {
    console.log("test123123123")
    return this.eventService.book(paymentMethodId, applicationId)
  }
}

