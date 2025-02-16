import { Injectable } from '@angular/core';
import { loadStripe, Stripe, StripeElements, StripeCardNumberElement } from '@stripe/stripe-js';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class StripeService {
  private stripe!: Stripe | null;
  private elements?: StripeElements;
  private cardNumberElement?: StripeCardNumberElement;

  constructor() {}

  async initStripe() {
    if (!this.stripe) {
      this.stripe = await loadStripe(environment.stripePublicKey);
      if (!this.stripe) {
        throw new Error('Failed to initialize Stripe.');
      }
    }
  }

  async createCardElements() {
    await this.initStripe();
    if (!this.elements) this.elements = this.stripe!.elements();
    if (!this.cardNumberElement) {
      this.cardNumberElement = this.elements.create('cardNumber', { showIcon: true });
      this.cardNumberElement.mount('#card-number-element');
    }
    this.elements.create('cardExpiry').mount('#card-expiry-element');
    this.elements.create('cardCvc').mount('#card-cvc-element');
  }

  async confirmPayment(clientSecret: string, email: string): Promise<void> {
    if (!this.stripe || !this.cardNumberElement) throw new Error('Stripe not initialized');
    console.log('Using clientSecret:', clientSecret);
    const result = await this.stripe.confirmCardPayment(clientSecret, {
      payment_method: {
        card: this.cardNumberElement,
        billing_details: { email }
      }
    });
    console.log('Payment result:', result);
    if (result.error) throw new Error(result.error.message);
  }

  async createPaymentMethod(email: string): Promise<string> {
    if (!this.stripe || !this.cardNumberElement) {
      throw new Error('Stripe not initialized');
    }
  
    const { paymentMethod, error } = await this.stripe.createPaymentMethod({
      type: 'card',
      card: this.cardNumberElement,
      billing_details: { email }
    });
  
    if (error) {
      console.error('Error creating PaymentMethod:', error.message);
      throw new Error(error.message);
    }
  
    console.log('Created PaymentMethod:', paymentMethod.id);
    return paymentMethod.id;  // Return the PaymentMethodId
  }
  
}
