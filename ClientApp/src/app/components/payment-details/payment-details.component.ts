import { Component } from '@angular/core';
import { loadStripe, Stripe } from '@stripe/stripe-js';
import { StripeService } from '../../services/stripe/stripe.service';
import { StripeAccountService } from '../../services/stripe-account/stripe-account.service';
import { StripeAccountToastService } from '../../services/toast/stripe-account/stripe-account-toast.service';
import { StripeToastService } from '../../services/toast/stripe-toast.service';
import { PopupService } from '../../services/popup/popup.service';

@Component({
  selector: 'app-payment-details',
  standalone: false,
  templateUrl: './payment-details.component.html',
  styleUrls: ['./payment-details.component.scss']
})
export class PaymentDetailsComponent {
  isVerified: boolean = false;

  constructor(
    private stripeAccountService: StripeAccountService,
    private stripeAccountToastService: StripeAccountToastService,
    private popupService: PopupService
  ) {}

  ngOnInit(): void {
    this.stripeAccountService.isUserVerified().subscribe({
      next: (verified) => this.isVerified = verified,
    });
  }

  startOnboarding() {
    this.stripeAccountService.getOnboardingLink().subscribe({
      next: (link: string) => {
        const popup = this.popupService.openPopup(link);
        this.popupService.monitorPopup(popup, () => {
          this.stripeAccountService.isUserVerified().subscribe({
            next: (verified) => {
              this.isVerified = verified;
              if (verified) {
                this.stripeAccountToastService.onboardingSuccess();
              } else {
                this.stripeAccountToastService.onboardingFail(); 
              }
            },
            error: () => this.stripeAccountToastService.onboardingFail()
          });
        });
        
      },
      error: () => this.stripeAccountToastService.onboardingError()
    });
  }
}
