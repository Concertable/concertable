import { useParams } from "@tanstack/react-router";
import dayjs from "dayjs";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { ImmediatePaymentSection } from "@/components/checkout/ImmediatePaymentSection";
import { CheckoutLayout } from "@/components/checkout/CheckoutLayout";
import { CheckoutSection } from "@/components/checkout/CheckoutSection";
import { OrderSummaryCard } from "@/components/checkout/OrderSummaryCard";
import { QuantitySelector } from "@/components/checkout/QuantitySelector";
import { CheckoutSuccessView } from "@/components/checkout/CheckoutSuccessView";
import { useConcert } from "@/hooks/useConcert";
import { usePaymentMethodQuery } from "@/hooks/query/useStripeAccountQuery";
import { useTicketCheckout } from "@/hooks/useTicketCheckout";
import type { TicketPurchasedPayload } from "@/types/notification";
import type { Concert } from "@/types/concert";

export default function ConcertCheckoutPage() {
  const { id } = useParams({ from: "/_customer/concert/checkout/$id" });
  const { concert, isLoading, isError } = useConcert(id);
  const { data: savedCard, isLoading: isPaymentMethodLoading } =
    usePaymentMethodQuery();
  const {
    quantity,
    setQuantity,
    paymentMethodId,
    setPaymentMethodId,
    state,
    error,
    result,
    purchase,
  } = useTicketCheckout(id);

  if (isLoading) return <CheckoutSkeleton />;
  if (isError || !concert)
    return <div className="text-destructive p-6">Concert not found.</div>;

  if (state === "success" && result)
    return <ConcertSuccessView result={result} concert={concert} />;

  const total = (concert.price * quantity).toFixed(2);
  const isDisabled = state === "pending" || state === "processing";

  return (
    <CheckoutLayout
      summary={
        <OrderSummaryCard
          lines={[
            {
              label: "Price per ticket",
              value: `£${concert.price.toFixed(2)}`,
            },
            { label: "Quantity", value: `×${quantity}` },
          ]}
          total={{ label: "Total", value: `£${total}` }}
          action={
            <Button
              className="w-full"
              size="lg"
              disabled={paymentMethodId === undefined || isDisabled}
              onClick={purchase}
            >
              {state === "pending" || state === "processing"
                ? "Processing..."
                : `Pay £${total}`}
            </Button>
          }
          footer={error && <p className="text-destructive text-sm">{error}</p>}
        />
      }
    >
      <CheckoutSection
        title={concert.name}
        description={`${concert.venue.name} · ${concert.venue.town}`}
      >
        <p className="text-muted-foreground text-sm">
          {dayjs(concert.startDate).format("dddd, D MMM YYYY · HH:mm")}
        </p>
      </CheckoutSection>

      <CheckoutSection
        title="Quantity"
        description={`${concert.availableTickets} ticket(s) available`}
      >
        <QuantitySelector
          value={quantity}
          onChange={setQuantity}
          max={concert.availableTickets}
          disabled={isDisabled}
        />
      </CheckoutSection>

      <CheckoutSection title="Payment Method">
        <ImmediatePaymentSection
          savedCard={savedCard}
          isLoading={isPaymentMethodLoading}
          onChange={setPaymentMethodId}
        />
      </CheckoutSection>
    </CheckoutLayout>
  );
}

function ConcertSuccessView({
  result,
  concert,
}: {
  result: TicketPurchasedPayload;
  concert: Concert;
}) {
  return (
    <CheckoutSuccessView
      title="Payment Successful"
      description={
        <>
          Your tickets for{" "}
          <span className="text-foreground font-medium">{concert.name}</span>{" "}
          are confirmed.
        </>
      }
      details={
        <div className="bg-card space-y-2 rounded-xl border p-5 text-left">
          <div className="flex justify-between text-sm">
            <span className="text-muted-foreground">Tickets</span>
            <span>{result.ticketIds.length}</span>
          </div>
          <div className="flex justify-between text-sm">
            <span className="text-muted-foreground">Total paid</span>
            <span>£{result.amount.toFixed(2)}</span>
          </div>
          <div className="flex justify-between text-sm">
            <span className="text-muted-foreground">Concert date</span>
            <span>{dayjs(concert.startDate).format("D MMM YYYY")}</span>
          </div>
          {result.transactionId && (
            <div className="flex justify-between text-sm">
              <span className="text-muted-foreground">Reference</span>
              <span className="font-mono text-xs">
                {result.transactionId.slice(0, 12)}...
              </span>
            </div>
          )}
        </div>
      }
      footer={
        result.userEmail && <>Tickets have been sent to {result.userEmail}</>
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
          <Skeleton className="h-32 w-full rounded-xl" />
          <Skeleton className="h-44 w-full rounded-xl" />
        </div>
        <Skeleton className="h-60 w-full rounded-xl" />
      </div>
    </div>
  );
}
