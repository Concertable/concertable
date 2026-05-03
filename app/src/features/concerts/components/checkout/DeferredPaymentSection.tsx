import { useEffect } from "react";
import { Skeleton } from "@/components/ui/skeleton";
import { AddPaymentMethodModal } from "@/features/payments";
import { SavedCardOption } from "./SavedCardOption";
import type { PaymentSectionProps } from "./PaymentMethodSection";

export function DeferredPaymentSection({
  savedCard,
  isLoading,
  onChange,
}: PaymentSectionProps) {
  useEffect(() => {
    if (!isLoading) onChange(savedCard?.id);
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
