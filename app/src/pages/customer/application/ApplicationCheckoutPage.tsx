import { useState } from "react";
import { useParams } from "@tanstack/react-router";
import { loadStripe } from "@stripe/stripe-js";
import dayjs from "dayjs";
import { CheckCircle } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import { Skeleton } from "@/components/ui/skeleton";
import { PaymentMethodSection } from "@/components/checkout/PaymentMethodSection";
import { AcceptContractSummary } from "@/components/applications/AcceptContractSummary";
import {
  useApplicationQuery,
  useAcceptApplicationMutation,
} from "@/hooks/query/useApplicationQuery";
import { usePaymentMethodQuery } from "@/hooks/query/useStripeAccountQuery";
import type { AcceptOutcome } from "@/types/application";

const stripePromise = loadStripe(import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY);

export default function ApplicationCheckoutPage() {
  const { applicationId } = useParams({ strict: false }) as {
    applicationId: number;
  };
  const {
    data: application,
    isLoading,
    isError,
  } = useApplicationQuery(applicationId);
  const { data: savedCard, isLoading: isPaymentMethodLoading } =
    usePaymentMethodQuery();
  const { mutate: accept, isPending } = useAcceptApplicationMutation(
    application?.opportunity.id ?? 0,
  );
  const [paymentMethodId, setPaymentMethodId] = useState<
    string | null | undefined
  >(undefined);
  const [isAwaiting, setIsAwaiting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (isLoading) return <CheckoutSkeleton />;
  if (isError || !application)
    return <div className="text-destructive p-6">Application not found.</div>;

  const { artist, opportunity } = application;

  async function handleOutcome(outcome: AcceptOutcome) {
    if (outcome.$type === "deferred") {
      setIsAwaiting(true);
      return;
    }

    if (outcome.payment.requiresAction && outcome.payment.clientSecret) {
      const stripe = await stripePromise;
      const { error } = await stripe!.handleNextAction({
        clientSecret: outcome.payment.clientSecret,
      });
      if (error) {
        setError(error.message ?? "Payment authentication failed.");
        return;
      }
    }

    setIsAwaiting(true);
  }

  function handleAccept() {
    setError(null);
    accept(
      { applicationId: application!.id, paymentMethodId },
      {
        onSuccess: handleOutcome,
        onError: () =>
          setError("Failed to accept application. Please try again."),
      },
    );
  }

  if (isAwaiting) {
    return (
      <div className="mx-auto flex max-w-lg flex-col items-center gap-4 p-6 pt-20 text-center">
        <CheckCircle className="mx-auto size-16 text-green-500" />
        <div className="space-y-1">
          <h1 className="text-2xl font-bold">Application Accepted</h1>
          <p className="text-muted-foreground">
            You have accepted{" "}
            <span className="text-foreground font-medium">{artist.name}</span>.
            Creating concert draft...
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-lg space-y-6 p-6">
      <h1 className="text-2xl font-bold">Checkout</h1>

      <div className="space-y-1 rounded-lg border p-4">
        <p className="font-semibold">{artist.name}</p>
        <p className="text-muted-foreground text-sm">
          {dayjs(opportunity.startDate).format("D MMM YYYY")} –{" "}
          {dayjs(opportunity.endDate).format("D MMM YYYY")}
        </p>
      </div>

      <Separator />

      <div className="space-y-3">
        <h2 className="font-medium">Contract Terms</h2>
        <div className="rounded-xl border p-4">
          <AcceptContractSummary contract={opportunity.contract} />
        </div>
      </div>

      <Separator />

      <div className="space-y-3">
        <h2 className="font-medium">Payment Method</h2>
        <PaymentMethodSection
          savedCard={savedCard}
          isLoading={isPaymentMethodLoading}
          onChange={setPaymentMethodId}
        />
      </div>

      {error && <p className="text-destructive text-sm">{error}</p>}

      <Button
        className="w-full"
        disabled={isPending || paymentMethodId === undefined}
        onClick={handleAccept}
      >
        {isPending ? "Processing..." : "Confirm & Pay"}
      </Button>
    </div>
  );
}

function CheckoutSkeleton() {
  return (
    <div className="mx-auto max-w-lg space-y-6 p-6">
      <Skeleton className="h-8 w-48" />
      <Skeleton className="h-16 w-full rounded-lg" />
      <Skeleton className="h-px w-full" />
      <Skeleton className="h-24 w-full rounded-lg" />
      <Skeleton className="h-px w-full" />
      <Skeleton className="h-[66px] w-full rounded-lg" />
      <Skeleton className="h-10 w-full rounded-lg" />
    </div>
  );
}
