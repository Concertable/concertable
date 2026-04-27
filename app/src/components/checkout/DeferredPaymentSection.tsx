import { useEffect } from "react";
import { CreditCard } from "lucide-react";
import { Skeleton } from "@/components/ui/skeleton";
import { AddPaymentMethodModal } from "@/components/AddPaymentMethodModal";
import type { PaymentSectionProps } from "@/components/checkout/PaymentMethodSection";

export function DeferredPaymentSection({
  savedCard,
  isLoading,
  onChange,
}: PaymentSectionProps) {
  useEffect(() => {
    if (!isLoading) onChange(savedCard ? null : undefined);
  }, [isLoading, savedCard]);

  if (isLoading) return <Skeleton className="h-[66px] w-full rounded-lg" />;

  return (
    <div className="space-y-3">
      {savedCard ? (
        <div className="border-primary bg-primary/5 rounded-lg border p-3">
          <div className="flex items-center gap-2">
            <CreditCard className="text-muted-foreground size-4 shrink-0" />
            <div>
              <p className="text-sm font-medium capitalize">
                {savedCard.brand} •••• {savedCard.last4}
              </p>
              <p className="text-muted-foreground text-xs">
                Expires {savedCard.expMonth}/{savedCard.expYear}
              </p>
            </div>
          </div>
        </div>
      ) : (
        <div className="rounded-lg border border-dashed p-4 text-center">
          <p className="text-muted-foreground mb-3 text-sm">
            A saved card is required for deferred payment.
          </p>
          <AddPaymentMethodModal />
        </div>
      )}

      {savedCard && (
        <div className="flex justify-end">
          <AddPaymentMethodModal replace />
        </div>
      )}
    </div>
  );
}
