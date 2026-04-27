import { useState, useEffect } from "react";
import { CreditCard } from "lucide-react";
import { Skeleton } from "@/components/ui/skeleton";
import { NewCardSection } from "@/components/NewCardSection";
import type { PaymentSectionProps } from "@/components/checkout/PaymentMethodSection";

enum PaymentOption {
  Saved = "saved",
  New = "new",
}

export function ImmediatePaymentSection({
  savedCard,
  isLoading,
  onChange,
}: PaymentSectionProps) {
  const [selected, setSelected] = useState<PaymentOption>(PaymentOption.Saved);

  useEffect(() => {
    if (!isLoading) {
      const initialOption = savedCard ? PaymentOption.Saved : PaymentOption.New;
      setSelected(initialOption);
      onChange(savedCard ? null : undefined);
    }
  }, [isLoading]);

  if (isLoading) return <Skeleton className="h-[66px] w-full rounded-lg" />;

  function selectSaved() {
    setSelected(PaymentOption.Saved);
    onChange(null);
  }

  function selectNew() {
    setSelected(PaymentOption.New);
    onChange(undefined);
  }

  return (
    <div className="space-y-3">
      <div className="grid grid-cols-2 gap-2">
        {savedCard && (
          <button
            onClick={selectSaved}
            className={`rounded-lg border p-3 text-left transition-colors ${
              selected === PaymentOption.Saved
                ? "border-primary bg-primary/5"
                : "hover:border-muted-foreground/50"
            }`}
          >
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
          </button>
        )}
        <button
          onClick={selectNew}
          className={`rounded-lg border p-3 text-left transition-colors ${
            selected === PaymentOption.New
              ? "border-primary bg-primary/5"
              : "hover:border-muted-foreground/50"
          } ${!savedCard ? "col-span-2" : ""}`}
        >
          <p className="text-sm font-medium">New card</p>
          <p className="text-muted-foreground text-xs">Enter card details</p>
        </button>
      </div>

      {selected === PaymentOption.New && (
        <NewCardSection onConfirmed={onChange} />
      )}
    </div>
  );
}
