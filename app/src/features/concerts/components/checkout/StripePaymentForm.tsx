import { useMemo, useState, type FormEvent, type ReactNode } from "react";
import {
  Elements,
  PaymentElement,
  useElements,
  useStripe,
} from "@stripe/react-stripe-js";
import type { Appearance, StripeElementsOptions } from "@stripe/stripe-js";
import { Button } from "@/components/ui/button";
import { stripePromise } from "@/lib/stripe";
import type { CheckoutSession } from "../../types";

const appearance: Appearance = { theme: "night" };

type Kind = "payment" | "setup";

export interface StripePaymentFormProps {
  session: CheckoutSession;
  kind: Kind;
  submitLabel: string;
  disabled?: boolean;
  footer?: ReactNode;
  onSuccess: () => void;
}

export function StripePaymentForm(props: StripePaymentFormProps) {
  const { clientSecret, customerSession } = props.session;
  const options = useMemo<StripeElementsOptions>(
    () => ({
      clientSecret,
      customerSessionClientSecret: customerSession,
      appearance,
    }),
    [clientSecret, customerSession],
  );

  return (
    <Elements stripe={stripePromise} options={options}>
      <Form {...props} />
    </Elements>
  );
}

function Form({
  kind,
  submitLabel,
  disabled,
  footer,
  onSuccess,
}: StripePaymentFormProps) {
  const stripe = useStripe();
  const elements = useElements();
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (!stripe || !elements) return;

    setIsSubmitting(true);
    setError(null);

    const { error: submitError } = await elements.submit();
    if (submitError) {
      setError(submitError.message ?? "Payment validation failed.");
      setIsSubmitting(false);
      return;
    }

    const result =
      kind === "payment"
        ? await stripe.confirmPayment({
            elements,
            redirect: "if_required",
          })
        : await stripe.confirmSetup({
            elements,
            redirect: "if_required",
          });

    if (result.error) {
      setError(result.error.message ?? "Payment failed.");
      setIsSubmitting(false);
      return;
    }

    onSuccess();
  }

  const isDisabled = !stripe || !elements || disabled || isSubmitting;

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <PaymentElement options={{ layout: "tabs" }} />
      {error && <p className="text-destructive text-sm">{error}</p>}
      <Button type="submit" className="w-full" size="lg" disabled={isDisabled}>
        {isSubmitting ? "Processing..." : submitLabel}
      </Button>
      {footer}
    </form>
  );
}
