import { useState } from "react";
import { useParams } from "@tanstack/react-router";
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

export default function ApplicationCheckoutPage() {
  const { id } = useParams({ from: "/_customer/application/checkout/$id" });
  const { data: application, isLoading, isError } = useApplicationQuery(id);
  const { data: savedCard, isLoading: isPaymentMethodLoading } =
    usePaymentMethodQuery();
  const { mutate: accept, isPending } = useAcceptApplicationMutation(
    application?.opportunity.id ?? 0,
  );
  const [paymentMethodId, setPaymentMethodId] = useState<
    string | null | undefined
  >(undefined);
  const [isSuccess, setIsSuccess] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (isLoading) return <CheckoutSkeleton />;
  if (isError || !application)
    return <div className="text-destructive p-6">Application not found.</div>;

  if (isSuccess) return <SuccessView artistName={application.artist.name} />;

  const { artist, opportunity } = application;

  function handleAccept() {
    setError(null);
    accept(application!.id, {
      onSuccess: () => setIsSuccess(true),
      onError: () =>
        setError("Failed to accept application. Please try again."),
    });
  }

  return (
    <div className="mx-auto max-w-lg space-y-6 p-6">
      <h1 className="text-2xl font-bold">Accept Application</h1>

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
        {isPending ? "Accepting..." : "Accept Application"}
      </Button>
    </div>
  );
}

function SuccessView({ artistName }: { artistName: string }) {
  return (
    <div className="mx-auto max-w-lg space-y-6 p-6 text-center">
      <CheckCircle className="mx-auto size-16 text-green-500" />
      <div className="space-y-1">
        <h1 className="text-2xl font-bold">Application Accepted</h1>
        <p className="text-muted-foreground">
          You have accepted{" "}
          <span className="text-foreground font-medium">{artistName}</span>.
          They will be notified and a draft concert will be created.
        </p>
      </div>
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
