import { loadStripe, type Stripe } from "@stripe/stripe-js";

let stripePromise: Promise<Stripe | null> | undefined;

// Keep lazy — never load Stripe.js at module top-level; its cookies must not fire before checkout.
export function getStripe(): Promise<Stripe | null> {
  stripePromise ??= loadStripe(import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY);
  return stripePromise;
}
