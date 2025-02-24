import { Component } from '@angular/core';
import { CheckoutDirective } from '../../directives/checkout.directive';
import { ListingApplication } from '../../models/listing-application';
import { ActivatedRoute, Router } from '@angular/router';
import { Listing } from '../../models/listing';
import { EventService } from '../../services/event/event.service';
import { Observable } from 'rxjs';
import { Purchase } from '../../models/purchase';
import { StripeService } from '../../services/stripe/stripe.service';
import { ListingApplicationPurchase } from '../../models/listing-application-purchase';

@Component({
  selector: 'app-listing-application-checkout',
  standalone: false,
  templateUrl: './listing-application-checkout.component.html',
  styleUrl: './listing-application-checkout.component.scss'
})
export class ListingApplicationCheckoutComponent extends CheckoutDirective<ListingApplication> {
  constructor(
    route: ActivatedRoute, 
    stripeService: StripeService, 
    private eventService: EventService,
    private router: Router
  ) {
    super(route, stripeService);
  }

  get application(): ListingApplication | undefined {
      return this.checkoutEntity;
    }
  
  set application(value: ListingApplication | undefined) {
    this.checkoutEntity = value;
  }

  setRouteData(data: any): void {
    console.log(data);
    this.application = data['listingApplication'];
    console.log("a",this.application);
  }

  checkout(paymentMethodId: string, applicationId: number): Observable<ListingApplicationPurchase> {
    return this.eventService.book(paymentMethodId, applicationId)
  }

  postCheckoutAction(): void {
    // if(this.application?.id)
    //   this.eventService.getDetailsByApplicationId(this.application.id).subscribe(e => {
    //     this.router.navigateByUrl(`venue/events/event/${e.id}`)
    //   })
  }
}

