import { useState } from "react";
import { useParams } from "@tanstack/react-router";
import dayjs from "dayjs";
import { Skeleton } from "@/components/ui/skeleton";
import { CheckoutLayout } from "@/components/checkout/CheckoutLayout";
import { CheckoutSection } from "@/components/checkout/CheckoutSection";
import { CheckoutEventBanner } from "@/components/checkout/CheckoutEventBanner";
import { OrderSummaryCard } from "@/components/checkout/OrderSummaryCard";
import { CheckoutSuccess } from "@/components/checkout/CheckoutSuccess";
import { StripePaymentForm } from "@/components/checkout/StripePaymentForm";
import {
  AcceptContractSummary,
  useApplicationQuery,
  useCheckoutQuery,
} from "@/features/concerts";
import { summaryFor } from "@/lib/acceptCheckoutFormat";

export default function ApplicationCheckoutPage() {
  const { applicationId } = useParams({ strict: false }) as {
    applicationId: number;
  };
  const {
    data: application,
    isLoading,
    isError,
  } = useApplicationQuery(applicationId);
  const {
    data: checkout,
    isLoading: isCheckoutLoading,
    isError: isCheckoutError,
  } = useCheckoutQuery(applicationId);
  const [isAwaiting, setIsAwaiting] = useState(false);

  if (isLoading || isCheckoutLoading) return <CheckoutSkeleton />;
  if (isError || !application)
    return <div className="text-destructive p-6">Application not found.</div>;
  if (isCheckoutError || !checkout)
    return (
      <div className="text-destructive p-6">Could not start checkout.</div>
    );

  const { artist, opportunity } = application;

  if (isAwaiting) {
    return (
      <CheckoutSuccess
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

  const summary = summaryFor(checkout.amount);
  const submitLabel =
    checkout.timing === "deferred" ? "Confirm" : "Confirm & Pay";

  return (
    <CheckoutLayout
      banner={
        <CheckoutEventBanner
          title={artist.name}
          subtitle={`${dayjs(opportunity.startDate).format("D MMM YYYY")} – ${dayjs(opportunity.endDate).format("D MMM YYYY")}`}
          meta={`Paying ${checkout.payee.name}${checkout.payee.email ? ` · ${checkout.payee.email}` : ""}`}
        />
      }
      summary={
        <OrderSummaryCard
          title={checkout.timing === "deferred" ? "Settlement" : "Summary"}
          lines={summary.lines}
          total={summary.total}
        />
      }
    >
      <CheckoutSection title="Contract Terms">
        <AcceptContractSummary contract={opportunity.contract} />
      </CheckoutSection>

      <CheckoutSection
        title="Payment Method"
        description={
          checkout.timing === "deferred"
            ? "Saved card required for settlement after the concert."
            : undefined
        }
      >
        <StripePaymentForm
          session={checkout.session}
          kind={checkout.timing === "deferred" ? "setup" : "payment"}
          submitLabel={submitLabel}
          onSuccess={() => setIsAwaiting(true)}
        />
      </CheckoutSection>
    </CheckoutLayout>
  );
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
