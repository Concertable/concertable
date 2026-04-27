import { useParams } from "@tanstack/react-router";
import dayjs from "dayjs";
import { CheckCircle, Minus, Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import { Skeleton } from "@/components/ui/skeleton";
import { ImmediatePaymentSection } from "@/components/checkout/ImmediatePaymentSection";
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
    return <SuccessView result={result} concert={concert} />;

  const total = (concert.price * quantity).toFixed(2);
  const isDisabled = state === "pending" || state === "processing";

  return (
    <div className="mx-auto max-w-lg space-y-6 p-6">
      <h1 className="text-2xl font-bold">Checkout</h1>

      <div className="space-y-1 rounded-lg border p-4">
        <p className="font-semibold">{concert.name}</p>
        <p className="text-muted-foreground text-sm">
          {concert.venue.name} · {concert.venue.town}
        </p>
        <p className="text-muted-foreground text-sm">
          {dayjs(concert.startDate).format("D MMM YYYY, HH:mm")}
        </p>
      </div>

      <Separator />

      <div className="space-y-3">
        <h2 className="font-medium">Quantity</h2>
        <div className="flex items-center gap-4">
          <Button
            variant="outline"
            size="icon"
            onClick={() => setQuantity((q) => Math.max(1, q - 1))}
            disabled={quantity <= 1 || isDisabled}
          >
            <Minus className="size-4" />
          </Button>
          <span className="w-6 text-center text-lg font-medium">
            {quantity}
          </span>
          <Button
            variant="outline"
            size="icon"
            onClick={() =>
              setQuantity((q) => Math.min(concert.availableTickets, q + 1))
            }
            disabled={quantity >= concert.availableTickets || isDisabled}
          >
            <Plus className="size-4" />
          </Button>
        </div>
      </div>

      <Separator />

      <div className="space-y-2">
        <div className="flex justify-between text-sm">
          <span className="text-muted-foreground">Price per ticket</span>
          <span>£{concert.price.toFixed(2)}</span>
        </div>
        <div className="flex justify-between text-sm">
          <span className="text-muted-foreground">Quantity</span>
          <span>×{quantity}</span>
        </div>
        <Separator />
        <div className="flex justify-between font-semibold">
          <span>Total</span>
          <span>£{total}</span>
        </div>
      </div>

      <Separator />

      <div className="space-y-3">
        <h2 className="font-medium">Payment Method</h2>
        <ImmediatePaymentSection
          savedCard={savedCard}
          isLoading={isPaymentMethodLoading}
          onChange={setPaymentMethodId}
        />
      </div>

      {error && <p className="text-destructive text-sm">{error}</p>}

      {state === "pending" ? (
        <div className="flex items-center gap-3 py-2">
          <div className="text-muted-foreground size-5 shrink-0 animate-spin rounded-full border-2 border-current border-t-transparent" />
          <p className="text-muted-foreground text-sm">
            Processing your payment...
          </p>
        </div>
      ) : (
        <Button
          className="w-full"
          disabled={paymentMethodId === undefined || isDisabled}
          onClick={purchase}
        >
          {state === "processing" ? "Processing..." : `Pay £${total}`}
        </Button>
      )}
    </div>
  );
}

function SuccessView({
  result,
  concert,
}: {
  result: TicketPurchasedPayload;
  concert: Concert;
}) {
  return (
    <div className="mx-auto max-w-lg space-y-6 p-6 text-center">
      <CheckCircle className="mx-auto size-16 text-green-500" />
      <div className="space-y-1">
        <h1 className="text-2xl font-bold">Payment Successful</h1>
        <p className="text-muted-foreground">
          Your tickets for{" "}
          <span className="text-foreground font-medium">{concert.name}</span>{" "}
          are confirmed.
        </p>
      </div>

      <div className="space-y-2 rounded-lg border p-4 text-left">
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

      {result.userEmail && (
        <p className="text-muted-foreground text-sm">
          Tickets have been sent to {result.userEmail}
        </p>
      )}
    </div>
  );
}

function CheckoutSkeleton() {
  return (
    <div className="mx-auto max-w-lg space-y-6 p-6">
      <Skeleton className="h-8 w-32" />
      <Skeleton className="h-20 w-full rounded-lg" />
      <Skeleton className="h-px w-full" />
      <Skeleton className="h-12 w-full rounded-lg" />
      <Skeleton className="h-px w-full" />
      <Skeleton className="h-16 w-full rounded-lg" />
      <Skeleton className="h-px w-full" />
      <Skeleton className="h-[66px] w-full rounded-lg" />
      <Skeleton className="h-10 w-full rounded-lg" />
    </div>
  );
}
