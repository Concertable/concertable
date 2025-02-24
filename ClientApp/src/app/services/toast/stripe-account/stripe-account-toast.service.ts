import { Injectable } from '@angular/core';
import { ToastService } from '../toast.service';
import { StripeError } from '@stripe/stripe-js';

@Injectable({
  providedIn: 'root'
})
export class StripeAccountToastService extends ToastService {
  addedBankAccount(accountId: string){
    this.showSuccess(`Successfully added bank account ${accountId}. Redirecting you to the stripe creation page`,"Account Added")
  }

  invalidSortCodeOrAccountNo(){
    this.showError("Invalid sort code or account number format");
  }

  bankTokenError(error: StripeError) {
    this.showError(`Error creating Bank Token: ${error}`)
  }

  onboardingWindowClosed() {
    this.showInfo("Stripe onboarding window closed");
  }
}
