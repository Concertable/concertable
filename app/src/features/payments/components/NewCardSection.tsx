import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { StripePaymentForm } from "@/features/concerts/components/checkout/StripePaymentForm";
import { useSetupIntentQuery } from "../hooks/useStripeAccountQuery";

interface Props {
  onConfirmed: (paymentMethodId: string) => void;
}

export function NewCardSection({ onConfirmed }: Props) {
  const {
    data: clientSecret,
    isLoading,
    isError,
    refetch,
  } = useSetupIntentQuery(true);

  return (
    <div className="rounded-lg border p-4">
      {isLoading && <Skeleton className="h-[120px] w-full rounded-lg" />}
      {isError && (
        <div className="space-y-2 text-sm">
          <p className="text-destructive">
            Couldn't initialise the payment form.
          </p>
          <Button size="sm" variant="outline" onClick={() => refetch()}>
            Try again
          </Button>
        </div>
      )}
      {clientSecret && (
        <StripePaymentForm
          session={{ clientSecret, intentType: "Setup" }}
          submitLabel="Use this card"
          onSuccess={onConfirmed}
        />
      )}
    </div>
  );
}
