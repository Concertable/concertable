import { useState, useEffect } from "react";
import { loadStripe } from "@stripe/stripe-js";
import {
  Elements,
  PaymentElement,
  useStripe,
  useElements,
} from "@stripe/react-stripe-js";
import { CreditCard } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { useTheme } from "@/providers/ThemeProvider";
import stripeAccountApi from "@/api/stripeAccountApi";
import type { PaymentMethod } from "@/api/stripeAccountApi";

const stripePromise = loadStripe(import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY);

type Option = "saved" | "new";

interface Props {
  savedCard: PaymentMethod | null | undefined;
  isLoading: boolean;
  onChange: (paymentMethodId: string | null | undefined) => void;
}

interface NewCardFormProps {
  onConfirmed: (paymentMethodId: string) => void;
}

function NewCardForm({ onConfirmed }: NewCardFormProps) {
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

export function PaymentMethodSection({ savedCard, isLoading, onChange }: Props) {
  const [selected, setSelected] = useState<Option>(savedCard ? "saved" : "new");
  const [clientSecret, setClientSecret] = useState<string | null>(null);
  const [isLoadingIntent, setIsLoadingIntent] = useState(false);
  const { theme } = useTheme();
  const isDark =
    theme === "dark" ||
    (theme === "system" && matchMedia("(prefers-color-scheme: dark)").matches);

  useEffect(() => {
    if (!isLoading) onChange(savedCard ? null : undefined);
  }, [isLoading]);

  if (isLoading) return <Skeleton className="h-[66px] w-full rounded-lg" />;

  async function selectNew() {
    setSelected("new");
    onChange(undefined);
    if (!clientSecret) {
      setIsLoadingIntent(true);
      const secret = await stripeAccountApi.createSetupIntent();
      setClientSecret(secret);
      setIsLoadingIntent(false);
    }
  }

  function selectSaved() {
    setSelected("saved");
    onChange(null);
  }

  function handleConfirmed(paymentMethodId: string) {
    onChange(paymentMethodId);
  }

  return (
    <div className="space-y-3">
      <div className="grid grid-cols-2 gap-2">
        {savedCard && (
          <button
            onClick={selectSaved}
            className={`rounded-lg border p-3 text-left transition-colors ${
              selected === "saved"
                ? "border-primary bg-primary/5"
                : "hover:border-muted-foreground/50"
            }`}
          >
            <div className="flex items-center gap-2">
              <CreditCard className="text-muted-foreground size-4 shrink-0" />
              <div>
                <p className="text-sm font-medium capitalize">
                  {savedCard.brand} •••• {savedCard.last4}
                </p>
                <p className="text-muted-foreground text-xs">
                  Expires {savedCard.expMonth}/{savedCard.expYear}
                </p>
              </div>
            </div>
          </button>
        )}
        <button
          onClick={selectNew}
          className={`rounded-lg border p-3 text-left transition-colors ${
            selected === "new"
              ? "border-primary bg-primary/5"
              : "hover:border-muted-foreground/50"
          } ${!savedCard ? "col-span-2" : ""}`}
        >
          <p className="text-sm font-medium">New card</p>
          <p className="text-muted-foreground text-xs">Enter card details</p>
        </button>
      </div>

      {selected === "new" && (
        <div className="rounded-lg border p-4">
          {isLoadingIntent || !clientSecret ? (
            <div className="text-muted-foreground py-4 text-center text-sm">
              Loading...
            </div>
          ) : (
            <Elements
              stripe={stripePromise}
              options={{
                clientSecret,
                appearance: { theme: isDark ? "night" : "stripe" },
              }}
            >
              <NewCardForm onConfirmed={handleConfirmed} />
            </Elements>
          )}
        </div>
      )}
    </div>
  );
}
