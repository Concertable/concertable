import { Injectable } from '@angular/core';
import { ToastService } from './toast.service';
import { ActiveToast, ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class StripeToastService extends ToastService {

  constructor(private router: Router, toastr: ToastrService) {
    super(toastr);
  }

  stripeNotInitialized() {
    this.showError("Stripe not initialized")
  }

  paymentMethodCreationError(errorMessage?: string) {
    this.showError(`Error creating PaymentMethod: ${errorMessage}`, "PaymentMethod Creation Error");
  }

  showTicketPurchase(ticketId: Number) {
    const toast: ActiveToast<any> = this.toastr.info(
      'View your Ticket by clicking here',
      'Ticket Purchased!',
      {
        closeButton: true,
        tapToDismiss: false,
        disableTimeOut: false,
      }
    );

    toast.onTap.subscribe(() => {
      this.router.navigate(['/profile/tickets/upcoming']);
      this.toastr.clear();
    });
  }
}
