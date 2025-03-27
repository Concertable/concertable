import { Component, EventEmitter, Input, Output } from '@angular/core';
import { StripeService } from '../../services/stripe/stripe.service';

@Component({
  selector: 'app-card-details',
  standalone: false,
  templateUrl: './card-details.component.html',
  styleUrl: './card-details.component.scss'
})
export class CardDetailsComponent {
  @Input() buttonName?: string = ''
  @Input() isProcessing?: boolean = false;
  @Output() paymentMethodChange = new EventEmitter<string>(); 
  message: string = '';

  constructor(private stripeService: StripeService) {}

  async ngOnInit() {
    try {
      await this.stripeService.createCardElements(); 
    } catch (error) {
      this.message = 'Failed to initialize payment fields.';
      console.error(error);
    }
  }

  async createPaymentMethod() {
    try {
      const paymentMethodId = await this.stripeService.createPaymentMethod();
      this.paymentMethodChange.emit(paymentMethodId); 
    } catch (error: any) {
      this.message = `Error creating payment method: ${error.message}`;
      console.error(error);
    }
  }
}
