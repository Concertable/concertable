import { Injectable } from '@angular/core';
import { ToastService } from './toast.service';

@Injectable({
  providedIn: 'root'
})
export class StripeToastService extends ToastService {
  stripeNotInitialized() {
    this.showError("Stripe not initialized")
  }

  paymentMethodCreationError(errorMessage?: string) {
    this.showError(`Error creating PaymentMethod: ${errorMessage}`, "PaymentMethod Creation Error");
  }
}
