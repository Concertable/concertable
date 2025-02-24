import { Injectable } from '@angular/core';
import { ToastService } from './toast.service';

@Injectable({
  providedIn: 'root'
})
export class StripeToastService extends ToastService {
  stripeNotInitialized() {
    this.showError("Stripe not initialized")
  }
}
