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
    const style = {
      base: {
        fontSize: '20px',  /* Larger text */
        color: '#333',
        fontFamily: 'Arial, sans-serif',
        padding: '12px', /* Bigger padding */
        borderRadius: '10px',
        background: '#f7f7f7',
        '::placeholder': {
          color: '#999',
          fontSize: '20px', /* Bigger placeholder */
        },
      },
      invalid: {
        color: '#e3342f',
      },
    };
    await this.initStripe();
    if (!this.elements) this.elements = this.stripe!.elements();
    if (!this.cardNumberElement) {
      this.cardNumberElement = this.elements.create('cardNumber', {
         showIcon: true,
         style
        });
      this.cardNumberElement.mount('#card-number-element');
    }
    this.elements.create('cardExpiry', { style }).mount('#card-expiry-element');
    this.elements.create('cardCvc', { style }).mount('#card-cvc-element');
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

  async createPaymentMethod(): Promise<string> {
    if (!this.stripe || !this.cardNumberElement) {
      throw new Error('Stripe not initialized');
    }
  
    const { paymentMethod, error } = await this.stripe.createPaymentMethod({
      type: 'card',
      card: this.cardNumberElement
    });
  
    if (error) {
      console.error('Error creating PaymentMethod:', error.message);
      throw new Error(error.message);
    }
  
    console.log('Created PaymentMethod:', paymentMethod.id);
    return paymentMethod.id;  // Return the PaymentMethodId
  }
  
}
