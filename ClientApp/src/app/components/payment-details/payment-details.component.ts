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
  stripe!: Stripe;
  accountHolderName: string = '';
  sortCode: string = '';
  accountNumber: string = '';
  showForm: boolean = false;
  isVerified: boolean = false;

  constructor(
    private stripeService: StripeService, 
    private stripeAccountService: StripeAccountService,
    private stripeToastService: StripeToastService,
    private stripeAccountToastService: StripeAccountToastService,
    private popupService: PopupService
  ) {}

  async ngOnInit() {
    this.stripe = await this.stripeService.getInstance();
    if (!this.stripe) {
      console.error('Stripe failed to initialize');
    }
    this.stripeAccountService.isUserVerified().subscribe(isVerified => this.isVerified = isVerified);
  }

  /** Ensure Sort Code is in XX-XX-XX format */
  formatSortCode() {
    this.sortCode = this.sortCode.replace(/\D/g, ''); // Remove non-numeric chars
    if (this.sortCode.length > 6) {
      this.sortCode = this.sortCode.slice(0, 6);
    }
    if (this.sortCode.length === 6) {
      this.sortCode = `${this.sortCode.slice(0, 2)}-${this.sortCode.slice(2, 4)}-${this.sortCode.slice(4, 6)}`;
    }
  }

  /** Ensure Account Number is exactly 8 digits */
  formatAccountNumber() {
    this.accountNumber = this.accountNumber.replace(/\D/g, ''); // Remove non-numeric chars
    if (this.accountNumber.length > 8) {
      this.accountNumber = this.accountNumber.slice(0, 8);
    }
  }

  /** Validate Sort Code Format */
  isValidSortCode(): boolean {
    return /^\d{2}-\d{2}-\d{2}$/.test(this.sortCode);
  }

  /** Validate Account Number Format */
  isValidAccountNumber(): boolean {
    return /^\d{8}$/.test(this.accountNumber);
  }

  showBankForm() {
    this.showForm = true;
  }

  async submitBankDetails() {
    if (!this.stripe) {
      this.stripeToastService.stripeNotInitialized();
      return;
    }

    if (!this.isValidSortCode() || !this.isValidAccountNumber()) {
      this.stripeAccountToastService.invalidSortCodeOrAccountNo();
      return;
    }

    const { token, error } = await this.stripe.createToken('bank_account', {
      country: "GB",
      currency: "GBP",
      routing_number: this.sortCode.replace(/-/g, ''), // Remove dashes
      account_number: this.accountNumber,
      account_holder_name: this.accountHolderName,
      account_holder_type: "individual"
    });

    if (error) {
      this.stripeAccountToastService.bankTokenError(error);
    } else {
      console.log("Bank Token:", token.id);
      this.stripeAccountService.addBankAccount(token.id).subscribe(response => {
        this.stripeAccountToastService.addedBankAccount(response.accountId);
        const popup = this.popupService.openPopup(response.redirectUri);
        this.popupService.monitorPopup(popup, () => this.stripeAccountToastService.onboardingWindowClosed())
      });
    }
  }
}
