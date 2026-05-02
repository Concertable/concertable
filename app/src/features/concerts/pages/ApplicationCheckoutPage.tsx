import { useState } from "react";
import { useParams, useRouter } from "@tanstack/react-router";
import dayjs from "dayjs";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import type { ConcertDraftCreatedPayload } from "@/features/notifications";
import {
  useApplicationQuery,
  useCheckoutQuery,
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
  } = useCheckoutQuery(applicationId);
  const [submitted, setSubmitted] = useState(false);

  if (isLoading || isCheckoutLoading) return <CheckoutSkeleton />;
  if (isError || !application)
    return <div className="text-destructive p-6">Application not found.</div>;
  if (isCheckoutError || !checkout)
    return (
      <div className="text-destructive p-6">Could not start checkout.</div>
    );

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
          kind={isDeferred ? "setup" : "payment"}
          submitLabel={isDeferred ? "Confirm" : "Confirm & Pay"}
          onSuccess={() => setSubmitted(true)}
        />
      </CheckoutSection>
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
