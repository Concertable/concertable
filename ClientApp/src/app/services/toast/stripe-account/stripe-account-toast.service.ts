import { Injectable } from '@angular/core';
import { ToastService } from '../toast.service';

@Injectable({
  providedIn: 'root'
})
export class StripeAccountToastService extends ToastService {
  onboardingStarted() {
    this.showSuccess("Redirecting you to complete your Stripe onboarding", "Stripe Onboarding");
  }

  onboardingSuccess() {
    this.showSuccess("Stripe Details Successfully added!", "Success")
  }

  onboardingFail() {
    this.showError("Failed to Authenticate Stripe Account", "Fail")
  }

  onboardingWindowClosed() {
    this.showInfo("Stripe onboarding window was closed");
  }

  onboardingError() {
    this.showError("Failed to generate Stripe onboarding link");
  }

  notVerified() {
    this.showError("User does not have a stripe Id")
  }
}
