import { Injectable } from '@angular/core';
import { loadStripe, Stripe, StripeElements, StripeCardNumberElement, StripeCardElement } from '@stripe/stripe-js';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class StripeService {
  private stripe!: Stripe | null;
  private elements?: StripeElements;
  private cardNumberElement?: StripeCardNumberElement;
  private cardElement?: StripeCardElement;

  constructor(private http: HttpClient) {}

  // Initialize Stripe
  async initStripe() {
    if (!this.stripe) {
      this.stripe = await loadStripe(environment.stripePublicKey);
      if (!this.stripe) {
        throw new Error('Failed to initialize Stripe.');
      }
    }
  }

  // Create Card Elements
  async createCardElements() {
    await this.initStripe();

    if (!this.elements) {
      this.elements = this.stripe!.elements();
    }

    // Create and mount the Card Number Element
    if (!this.cardNumberElement) {
      this.cardNumberElement = this.elements.create('cardNumber', {
        showIcon: true
      });
      this.cardNumberElement.mount('#card-number-element');
    }

    // Create and mount the Expiry Element
    const cardExpiry = this.elements.create('cardExpiry');
    cardExpiry.mount('#card-expiry-element');

    // Create and mount the CVC Element
    const cardCvc = this.elements.create('cardCvc');
    cardCvc.mount('#card-cvc-element');
  }

  // Create Payment Method
  async createPaymentMethod(email: string): Promise<string> {
    if (!this.stripe || !this.cardNumberElement) {
      throw new Error('Stripe Elements not initialized');
    }

    const { paymentMethod, error } = await this.stripe.createPaymentMethod({
      type: 'card',
      card: this.cardNumberElement,
      billing_details: { email }
    });

    if (error) {
      throw new Error(error.message);
    }

    return paymentMethod.id;
  }

}
