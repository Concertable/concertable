import { useState } from "react";
import { loadStripe } from "@stripe/stripe-js";
import {
  Elements,
  PaymentElement,
  useStripe,
  useElements,
} from "@stripe/react-stripe-js";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { useTheme } from "@/providers/ThemeProvider";
import { useSetupIntentQuery } from "@/hooks/query/useStripeAccountQuery";

const stripePromise = loadStripe(import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY);

interface Props {
  onConfirmed: (paymentMethodId: string) => void;
}

function NewCardForm({ onConfirmed }: Props) {
  const stripe = useStripe();
  const elements = useElements();
  const [isConfirming, setIsConfirming] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleConfirm() {
    if (!stripe || !elements) return;
    setIsConfirming(true);
    setError(null);

    const { error: stripeError, setupIntent } = await stripe.confirmSetup({
      elements,
      confirmParams: { return_url: window.location.href },
      redirect: "if_required",
    });

    if (stripeError) {
      setError(stripeError.message ?? "Something went wrong");
      setIsConfirming(false);
    } else if (setupIntent?.payment_method) {
      onConfirmed(setupIntent.payment_method as string);
    }
  }

  return (
    <div className="space-y-4">
      <PaymentElement options={{ wallets: { link: "never" } }} />
      {error && <p className="text-destructive text-sm">{error}</p>}
      <Button
        onClick={handleConfirm}
        disabled={isConfirming || !stripe}
        className="w-full"
        variant="outline"
      >
        {isConfirming ? "Confirming..." : "Use this card"}
      </Button>
    </div>
  );
}

export function NewCardSection({ onConfirmed }: Props) {
  const { theme } = useTheme();
  const isDark =
    theme === "dark" ||
    (theme === "system" && matchMedia("(prefers-color-scheme: dark)").matches);

  const {
    data: clientSecret,
    isLoading,
    isError,
    refetch,
  } = useSetupIntentQuery(true);

  return (
    <div className="rounded-lg border p-4">
      {isLoading && <Skeleton className="h-[120px] w-full rounded-lg" />}
      {isError && (
        <div className="space-y-2 text-sm">
          <p className="text-destructive">
            Couldn't initialise the payment form.
          </p>
          <Button size="sm" variant="outline" onClick={() => refetch()}>
            Try again
          </Button>
        </div>
      )}
      {clientSecret && (
        <Elements
          stripe={stripePromise}
          options={{
            clientSecret,
            appearance: { theme: isDark ? "night" : "stripe" },
          }}
        >
          <NewCardForm onConfirmed={onConfirmed} />
        </Elements>
      )}
    </div>
  );
}
