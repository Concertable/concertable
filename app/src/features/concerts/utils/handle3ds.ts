import { stripePromise } from "@/lib/stripe";

export interface PaymentChallenge {
  requiresAction: boolean;
  clientSecret?: string | null;
}

export async function handle3ds(payment: PaymentChallenge | null | undefined): Promise<void> {
  if (!payment?.requiresAction || !payment.clientSecret) return;

  const stripe = await stripePromise;
  if (!stripe) throw new Error("Stripe failed to load. Please refresh and try again.");

  const { error } = await stripe.handleNextAction({ clientSecret: payment.clientSecret });
  if (error) throw new Error(error.message ?? "Payment authentication failed.");
}
