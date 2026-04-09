import { useState } from "react";
import { loadStripe } from "@stripe/stripe-js";
import {
  Elements,
  PaymentElement,
  useStripe,
  useElements,
} from "@stripe/react-stripe-js";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { createSetupIntent } from "@/api/stripeAccountApi";

const stripe = loadStripe(import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY);

function PaymentForm({ onSuccess }: { onSuccess: () => void }) {
  const stripeInstance = useStripe();
  const elements = useElements();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!stripeInstance || !elements) return;

    setIsSubmitting(true);
    setError(null);

    const { error: stripeError } = await stripeInstance.confirmSetup({
      elements,
      confirmParams: { return_url: window.location.href },
      redirect: "if_required",
    });

    if (stripeError) {
      setError(stripeError.message ?? "Something went wrong");
      setIsSubmitting(false);
    } else {
      onSuccess();
    }
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <PaymentElement />
      {error && <p className="text-destructive text-sm">{error}</p>}
      <Button
        type="submit"
        disabled={isSubmitting || !stripeInstance}
        className="w-full"
      >
        {isSubmitting ? "Saving..." : "Save Payment Method"}
      </Button>
    </form>
  );
}

interface AddPaymentMethodModalProps {
  open: boolean;
  onClose: () => void;
}

export function AddPaymentMethodModal({
  open,
  onClose,
}: AddPaymentMethodModalProps) {
  const [clientSecret, setClientSecret] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  async function handleOpenChange(isOpen: boolean) {
    if (isOpen) {
      setIsLoading(true);
      const secret = await createSetupIntent();
      setClientSecret(secret);
      setIsLoading(false);
    } else {
      setClientSecret(null);
      onClose();
    }
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Add Payment Method</DialogTitle>
        </DialogHeader>
        {isLoading || !clientSecret ? (
          <div className="text-muted-foreground py-8 text-center text-sm">
            Loading...
          </div>
        ) : (
          <Elements stripe={stripe} options={{ clientSecret }}>
            <PaymentForm onSuccess={onClose} />
          </Elements>
        )}
      </DialogContent>
    </Dialog>
  );
}
