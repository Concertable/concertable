import { useState, useEffect } from "react";
import { Skeleton } from "@/components/ui/skeleton";
import { SavedCardOption } from "./SavedCardOption";
import { NewCardOption } from "./NewCardOption";
import type { PaymentSectionProps } from "./PaymentMethodSection";

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
      setSelected(savedCard ? PaymentOption.Saved : PaymentOption.New);
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
    <div className="grid grid-cols-2 gap-2">
      {savedCard && (
        <SavedCardOption
          card={savedCard}
          selected={selected === PaymentOption.Saved}
          onClick={selectSaved}
        />
      )}
      <NewCardOption
        selected={selected === PaymentOption.New}
        onClick={selectNew}
        onConfirmed={onChange}
        fullWidth={!savedCard}
      />
    </div>
  );
}
