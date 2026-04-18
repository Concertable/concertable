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
  DialogTrigger,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import stripeAccountApi from "@/api/stripeAccountApi";
import { useTheme } from "@/providers/ThemeProvider";

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
      toast.success("Payment method saved");
      onSuccess();
    }
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <PaymentElement options={{ wallets: { link: "never" } }} />
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

export function AddPaymentMethodModal({
  replace = false,
}: {
  replace?: boolean;
}) {
  const [open, setOpen] = useState(false);
  const [clientSecret, setClientSecret] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const queryClient = useQueryClient();
  const { theme } = useTheme();
  const isDark =
    theme === "dark" ||
    (theme === "system" && matchMedia("(prefers-color-scheme: dark)").matches);

  async function handleOpenChange(isOpen: boolean) {
    setOpen(isOpen);
    if (isOpen) {
      setIsLoading(true);
      const secret = await stripeAccountApi.createSetupIntent();
      setClientSecret(secret);
      setIsLoading(false);
    } else {
      setClientSecret(null);
    }
  }

  function handleSuccess() {
    setOpen(false);
    queryClient.invalidateQueries({ queryKey: ["stripe", "payment-method"] });
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogTrigger asChild>
        <Button variant={replace ? "outline" : "default"}>
          {replace ? "Replace" : "Add Payment Method"}
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Add Payment Method</DialogTitle>
        </DialogHeader>
        {isLoading || !clientSecret ? (
          <div className="text-muted-foreground py-8 text-center text-sm">
            Loading...
          </div>
        ) : (
          <Elements
            stripe={stripe}
            options={{
              clientSecret,
              appearance: { theme: isDark ? "night" : "stripe" },
            }}
          >
            <PaymentForm onSuccess={handleSuccess} />
          </Elements>
        )}
      </DialogContent>
    </Dialog>
  );
}
