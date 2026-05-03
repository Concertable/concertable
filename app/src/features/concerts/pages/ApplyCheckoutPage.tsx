import { useState } from "react";
import { useParams } from "@tanstack/react-router";
import { useMutation } from "@tanstack/react-query";
import { Skeleton } from "@/components/ui/skeleton";
import { useApplyCheckoutQuery } from "../hooks/useApplicationQuery";
import applicationApi from "../api/applicationApi";
import { CheckoutAwaiting } from "../components/checkout/CheckoutAwaiting";
import { CheckoutLayout } from "../components/checkout/CheckoutLayout";
import { CheckoutSection } from "../components/checkout/CheckoutSection";
import { CheckoutEventBanner } from "../components/checkout/CheckoutEventBanner";
import { OrderSummaryCard } from "../components/checkout/OrderSummaryCard";
import { CheckoutSuccess } from "../components/checkout/CheckoutSuccess";
import { StripePaymentForm } from "../components/checkout/StripePaymentForm";
import { summaryFor } from "../utils/acceptCheckoutFormat";
import type { Checkout } from "../types";

export function ApplyCheckoutPage() {
  const { opportunityId } = useParams({ strict: false }) as {
    opportunityId: number;
  };
  const {
    data: checkout,
    isLoading,
    isError,
  } = useApplyCheckoutQuery(opportunityId);

  if (isLoading) return <CheckoutSkeleton />;
  if (isError || !checkout)
    return (
      <div className="text-destructive p-6">Could not start checkout.</div>
    );

  return <ApplyCheckoutForm opportunityId={opportunityId} checkout={checkout} />;
}

function ApplyCheckoutForm({
  opportunityId,
  checkout,
}: {
  opportunityId: number;
  checkout: Checkout;
}) {
  const [submitted, setSubmitted] = useState(false);
  const applyMutation = useMutation({
    mutationFn: (paymentMethodId: string) =>
      applicationApi.applyToOpportunity(opportunityId, paymentMethodId),
    onSuccess: () => setSubmitted(true),
  });

  if (submitted)
    return (
      <CheckoutSuccess
        title="Application Submitted"
        description={
          <>
            Your card was authorised and your application was sent to{" "}
            <span className="text-foreground font-medium">
              {checkout.payee.name}
            </span>
            .
          </>
        }
      />
    );

  if (applyMutation.isPending)
    return (
      <CheckoutAwaiting
        title="Submitting application"
        description={`Sending your application to ${checkout.payee.name}`}
        steps={[
          { label: "Card authorised", status: "done" },
          { label: "Submitting application", status: "active" },
        ]}
      />
    );

  const summary = summaryFor(checkout.amount);

  return (
    <CheckoutLayout
      banner={
        <CheckoutEventBanner
          title={checkout.payee.name}
          subtitle="Authorise card to apply"
          meta={checkout.payee.email ?? undefined}
        />
      }
      summary={
        <OrderSummaryCard
          title="Hire Fee"
          lines={summary.lines}
          total={summary.total}
        />
      }
    >
      <CheckoutSection
        title="Payment Method"
        description="The venue will only charge this card if your application is accepted."
      >
        <StripePaymentForm
          session={checkout.session}
          submitLabel="Authorise & Apply"
          onSuccess={(paymentMethodId) => applyMutation.mutate(paymentMethodId)}
        />
      </CheckoutSection>
      {applyMutation.error && (
        <p className="text-destructive text-sm">{applyMutation.error.message}</p>
      )}
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
          <Skeleton className="h-44 w-full rounded-xl" />
        </div>
        <Skeleton className="h-60 w-full rounded-xl" />
      </div>
    </div>
  );
}
