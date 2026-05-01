import { useEffect, useState } from "react";
import { useParams, useRouter } from "@tanstack/react-router";
import dayjs from "dayjs";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { CheckoutLayout } from "@/components/checkout/CheckoutLayout";
import { CheckoutSection } from "@/components/checkout/CheckoutSection";
import { CheckoutEventBanner } from "@/components/checkout/CheckoutEventBanner";
import { OrderSummaryCard } from "@/components/checkout/OrderSummaryCard";
import { QuantitySelector } from "@/components/checkout/QuantitySelector";
import { CheckoutSuccess } from "@/components/checkout/CheckoutSuccess";
import {
  CheckoutAwaiting,
  type AwaitingStep,
} from "@/components/checkout/CheckoutAwaiting";
import { StripePaymentForm } from "@/components/checkout/StripePaymentForm";
import { useConcert } from "@/features/concerts";
import { useTicketCheckoutQuery } from "@/hooks/query/useTicketsQuery";
import { notificationConnection } from "@/lib/signalr";
import type { TicketPurchasedPayload } from "@/features/notifications";
import type { Concert } from "@/features/concerts";

type CheckoutPhase = "form" | "awaiting" | "success" | "timeout";

export default function ConcertCheckoutPage() {
  const { id } = useParams({ from: "/_customer/concert/checkout/$id" });
  const router = useRouter();
  const { concert, isLoading, isError } = useConcert(id);
  const {
    data: checkout,
    isLoading: isCheckoutLoading,
    isError: isCheckoutError,
  } = useTicketCheckoutQuery(id);

  const [quantity, setQuantity] = useState(1);
  const [phase, setPhase] = useState<CheckoutPhase>("form");
  const [ticketCount, setTicketCount] = useState(0);

  useEffect(() => {
    if (phase !== "awaiting") return;

    function handleTicketPurchased(payload: TicketPurchasedPayload) {
      if (payload.concertId !== id) return;
      setTicketCount(payload.ticketIds.length);
      setPhase("success");
    }

    notificationConnection.on("TicketPurchased", handleTicketPurchased);
    const timeoutId = setTimeout(() => {
      setPhase((current) => (current === "awaiting" ? "timeout" : current));
    }, 30_000);

    return () => {
      notificationConnection.off("TicketPurchased", handleTicketPurchased);
      clearTimeout(timeoutId);
    };
  }, [phase, id]);

  if (isLoading || isCheckoutLoading) return <CheckoutSkeleton />;
  if (isError || !concert)
    return <div className="text-destructive p-6">Concert not found.</div>;
  if (isCheckoutError || !checkout)
    return (
      <div className="text-destructive p-6">Could not start checkout.</div>
    );

  if (phase === "awaiting") {
    const steps: AwaitingStep[] = [
      { label: "Payment authorised", status: "done" },
      { label: "Confirming with our system", status: "active" },
      { label: "Issuing your tickets", status: "pending" },
    ];
    return (
      <CheckoutAwaiting
        title="Processing your payment"
        description="This usually takes a few seconds. Please don't close this page."
        steps={steps}
      />
    );
  }

  if (phase === "success") {
    return (
      <ConcertSuccess
        concert={concert}
        ticketCount={ticketCount}
        onView={() => void router.navigate({ to: "/profile/tickets/upcoming" })}
      />
    );
  }

  if (phase === "timeout") {
    return (
      <CheckoutAwaiting
        title="Still confirming your payment"
        description="This is taking longer than usual. Your tickets will appear in your profile shortly — feel free to keep using the app."
        steps={[
          { label: "Payment authorised", status: "done" },
          { label: "Confirming with our system", status: "active" },
          { label: "Issuing your tickets", status: "pending" },
        ]}
      />
    );
  }

  const total = concert.price * quantity;

  return (
    <CheckoutLayout
      banner={
        <CheckoutEventBanner
          title={concert.name}
          subtitle={`${concert.venue.name} · ${concert.venue.town}`}
          meta={dayjs(concert.startDate).format("dddd, D MMM YYYY · HH:mm")}
        />
      }
      summary={
        <OrderSummaryCard
          lines={[
            {
              label: "Price per ticket",
              value: `£${concert.price.toFixed(2)}`,
            },
            {
              label: "Quantity",
              value: (
                <QuantitySelector
                  value={quantity}
                  onChange={setQuantity}
                  max={concert.availableTickets}
                />
              ),
            },
          ]}
          total={{ label: "Total", value: `£${total.toFixed(2)}` }}
        />
      }
    >
      <CheckoutSection title="Payment Method">
        <StripePaymentForm
          session={checkout.session}
          kind="payment"
          submitLabel={`Pay £${total.toFixed(2)}`}
          onSuccess={() => setPhase("awaiting")}
        />
      </CheckoutSection>
    </CheckoutLayout>
  );
}

function ConcertSuccess({
  concert,
  ticketCount,
  onView,
}: {
  concert: Concert;
  ticketCount: number;
  onView: () => void;
}) {
  return (
    <CheckoutSuccess
      title="Tickets confirmed"
      description={
        <>
          Your {ticketCount > 1 ? `${ticketCount} tickets` : "ticket"} for{" "}
          <span className="text-foreground font-medium">{concert.name}</span>{" "}
          {ticketCount > 1 ? "are" : "is"} ready.
        </>
      }
      footer={
        <Button onClick={onView} className="mt-2">
          View tickets
        </Button>
      }
    />
  );
}

function CheckoutSkeleton() {
  return (
    <div className="mx-auto max-w-6xl space-y-6 px-6 py-8 lg:px-10 lg:py-10">
      <Skeleton className="h-8 w-32" />
      <Skeleton className="h-16 w-full" />
      <div className="grid gap-10 lg:grid-cols-[minmax(0,1fr)_400px] lg:gap-16">
        <Skeleton className="h-44 w-full rounded-lg" />
        <Skeleton className="h-72 w-full rounded-lg" />
      </div>
    </div>
  );
}
