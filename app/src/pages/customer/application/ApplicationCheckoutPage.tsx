import { useState } from "react";
import { useParams } from "@tanstack/react-router";
import { loadStripe } from "@stripe/stripe-js";
import dayjs from "dayjs";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { PaymentMethodSection } from "@/components/checkout/PaymentMethodSection";
import { CheckoutLayout } from "@/components/checkout/CheckoutLayout";
import { CheckoutSection } from "@/components/checkout/CheckoutSection";
import { CheckoutEventBanner } from "@/components/checkout/CheckoutEventBanner";
import { OrderSummaryCard } from "@/components/checkout/OrderSummaryCard";
import { CheckoutSuccessView } from "@/components/checkout/CheckoutSuccessView";
import { AcceptContractSummary } from "@/components/applications/AcceptContractSummary";
import {
  useApplicationQuery,
  useAcceptApplicationMutation,
  useAcceptPreviewQuery,
} from "@/hooks/query/useApplicationQuery";
import { usePaymentMethodQuery } from "@/hooks/query/useStripeAccountQuery";
import type { AcceptOutcome } from "@/types/application";
import type { AcceptPreview, PaymentAmount } from "@/types/acceptPreview";

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
  const { data: preview, isLoading: isPreviewLoading } =
    useAcceptPreviewQuery(applicationId);
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

  if (isLoading || isPreviewLoading) return <CheckoutSkeleton />;
  if (isError || !application || !preview)
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
      <CheckoutSuccessView
        title="Application Accepted"
        description={
          <>
            You have accepted{" "}
            <span className="text-foreground font-medium">{artist.name}</span>.
            Creating concert draft...
          </>
        }
      />
    );
  }

  const ctaLabel = preview.timing === "deferred" ? "Confirm" : "Confirm & Pay";

  return (
    <CheckoutLayout
      banner={
        <CheckoutEventBanner
          title={artist.name}
          subtitle={`${dayjs(opportunity.startDate).format("D MMM YYYY")} – ${dayjs(opportunity.endDate).format("D MMM YYYY")}`}
          meta={`Paying ${preview.payee.name}${preview.payee.email ? ` · ${preview.payee.email}` : ""}`}
        />
      }
      summary={
        <OrderSummaryCard
          title={preview.timing === "deferred" ? "Settlement" : "Summary"}
          lines={buildSummaryLines(preview)}
          total={buildTotalLine(preview)}
          action={
            <Button
              className="w-full"
              size="lg"
              disabled={isPending || paymentMethodId === undefined}
              onClick={handleAccept}
            >
              {isPending ? "Processing..." : ctaLabel}
            </Button>
          }
          footer={error && <p className="text-destructive text-sm">{error}</p>}
        />
      }
    >
      <CheckoutSection title="Contract Terms">
        <AcceptContractSummary contract={opportunity.contract} />
      </CheckoutSection>

      <CheckoutSection
        title="Payment Method"
        description={
          preview.timing === "deferred"
            ? "Saved card required for settlement after the concert."
            : undefined
        }
      >
        <PaymentMethodSection
          timing={preview.timing}
          savedCard={savedCard}
          isLoading={isPaymentMethodLoading}
          onChange={setPaymentMethodId}
        />
      </CheckoutSection>
    </CheckoutLayout>
  );
}

function buildSummaryLines(preview: AcceptPreview) {
  const payeeLine = { label: "Payee", value: preview.payee.name };
  return [payeeLine, ...amountLines(preview.amount)];
}

function buildTotalLine(preview: AcceptPreview): {
  label: string;
  value: string;
} {
  if (preview.timing === "deferred")
    return {
      label: "Settled at concert end",
      value: estimateLabel(preview.amount),
    };
  return { label: "Total due now", value: `£${flatTotal(preview.amount)}` };
}

function amountLines(amount: PaymentAmount) {
  switch (amount.$type) {
    case "flat":
      return [{ label: "Fee", value: `£${amount.amount.toFixed(2)}` }];
    case "doorShare":
      return [
        { label: "Artist door share", value: `${amount.artistPercent}%` },
      ];
    case "guaranteedDoor":
      return [
        { label: "Guarantee", value: `£${amount.guarantee.toFixed(2)}` },
        { label: "Artist door share", value: `${amount.artistPercent}%` },
      ];
  }
}

function flatTotal(amount: PaymentAmount): string {
  if (amount.$type === "flat") return amount.amount.toFixed(2);
  return "—";
}

function estimateLabel(amount: PaymentAmount): string {
  if (amount.$type === "doorShare") return `${amount.artistPercent}% of door`;
  if (amount.$type === "guaranteedDoor")
    return `£${amount.guarantee.toFixed(2)} + ${amount.artistPercent}% door`;
  return "—";
}

function CheckoutSkeleton() {
  return (
    <div className="mx-auto max-w-5xl space-y-6 px-4 py-8 lg:py-12">
      <Skeleton className="h-9 w-40" />
      <div className="grid gap-6 lg:grid-cols-[minmax(0,1fr)_380px] lg:gap-8">
        <div className="space-y-6">
          <Skeleton className="h-28 w-full rounded-xl" />
          <Skeleton className="h-40 w-full rounded-xl" />
          <Skeleton className="h-44 w-full rounded-xl" />
        </div>
        <Skeleton className="h-60 w-full rounded-xl" />
      </div>
    </div>
  );
}
