import { useEffect } from "react";
import { Skeleton } from "@/components/ui/skeleton";
import { AddPaymentMethodModal } from "@/components/AddPaymentMethodModal";
import { SavedCardOption } from "@/components/checkout/SavedCardOption";
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
        <SavedCardOption card={savedCard} selected />
      ) : (
        <p className="text-muted-foreground text-sm">
          A saved card is required for deferred payment.
        </p>
      )}
      <div className="flex justify-end">
        <AddPaymentMethodModal replace={!!savedCard} />
      </div>
    </div>
  );
}
