import { useState } from "react";
import { useParams, useRouter } from "@tanstack/react-router";
import dayjs from "dayjs";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import type { ConcertDraftCreatedPayload } from "@/features/notifications";
import type {
  Application,
  Checkout,
} from "../types";
import {
  useAcceptApplicationMutation,
  useAcceptCheckoutQuery,
  useApplicationQuery,
} from "../hooks/useApplicationQuery";
import { useCheckoutFlow } from "../hooks/useCheckoutFlow";
import { AcceptContractSummary } from "../components/applications/AcceptContractSummary";
import { CheckoutLayout } from "../components/checkout/CheckoutLayout";
import { CheckoutSection } from "../components/checkout/CheckoutSection";
import { CheckoutEventBanner } from "../components/checkout/CheckoutEventBanner";
import { OrderSummaryCard } from "../components/checkout/OrderSummaryCard";
import { CheckoutSuccess } from "../components/checkout/CheckoutSuccess";
import { CheckoutFlow } from "../components/checkout/CheckoutFlow";
import { StripePaymentForm } from "../components/checkout/StripePaymentForm";
import { summaryFor } from "../utils/acceptCheckoutFormat";
import { handle3ds } from "../utils/handle3ds";

export function ApplicationCheckoutPage() {
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
  } = useAcceptCheckoutQuery(applicationId);

  if (isLoading || isCheckoutLoading) return <CheckoutSkeleton />;
  if (isError || !application)
    return <div className="text-destructive p-6">Application not found.</div>;
  if (isCheckoutError || !checkout)
    return (
      <div className="text-destructive p-6">Could not start checkout.</div>
    );

  return (
    <ApplicationCheckoutForm
      applicationId={applicationId}
      application={application}
      checkout={checkout}
    />
  );
}

function ApplicationCheckoutForm({
  applicationId,
  application,
  checkout,
}: {
  applicationId: number;
  application: Application;
  checkout: Checkout;
}) {
  const [submitted, setSubmitted] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const acceptMutation = useAcceptApplicationMutation(application.opportunity.id);

  const { artist, opportunity } = application;
  const isDeferred = checkout.timing === "Deferred";

  if (submitted)
    return (
      <ApplicationCheckoutResolution
        artistName={artist.name}
        firstStep={isDeferred ? "Card saved" : "Payment authorised"}
      />
    );

  const summary = summaryFor(checkout.amount);

  async function handleAccept(paymentMethodId: string) {
    setError(null);
    try {
      const outcome = await acceptMutation.mutateAsync({ applicationId, paymentMethodId });
      if (outcome.$type === "immediate")
        await handle3ds(outcome.payment);
      setSubmitted(true);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Acceptance failed. Please try again.");
    }
  }

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
          title={isDeferred ? "Settlement" : "Summary"}
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
          isDeferred
            ? "Saved card required for settlement after the concert."
            : undefined
        }
      >
        <StripePaymentForm
          session={checkout.session}
          submitLabel={isDeferred ? "Confirm" : "Confirm & Pay"}
          disabled={acceptMutation.isPending}
          onSuccess={handleAccept}
        />
      </CheckoutSection>
      {error && <p className="text-destructive text-sm">{error}</p>}
    </CheckoutLayout>
  );
}

function ApplicationCheckoutResolution({
  artistName,
  firstStep,
}: {
  artistName: string;
  firstStep: string;
}) {
  const router = useRouter();
  const flow = useCheckoutFlow<ConcertDraftCreatedPayload>({
    event: "ConcertDraftCreated",
  });

  return (
    <CheckoutFlow
      flow={flow}
      title="Finalising acceptance"
      timeoutTitle="Still finalising"
      pendingHint="Your concert will appear in your dashboard"
      steps={{ first: firstStep, final: "Creating concert draft" }}
      renderSuccess={(concertId) => (
        <ApplicationSuccess
          artistName={artistName}
          onView={() =>
            void router.navigate({
              to: "/venue/my/concerts/concert/$id",
              params: { id: concertId },
            })
          }
        />
      )}
    />
  );
}

function ApplicationSuccess({
  artistName,
  onView,
}: {
  artistName: string;
  onView: () => void;
}) {
  return (
    <CheckoutSuccess
      title="Application Accepted"
      description={
        <>
          You have accepted{" "}
          <span className="text-foreground font-medium">{artistName}</span>.
          Your concert draft is ready.
        </>
      }
      footer={
        <Button onClick={onView} className="mt-2">
          View concert
        </Button>
      }
    />
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
