import { CreditCard } from "lucide-react";
import type { PaymentMethod } from "@/api/stripeAccountApi";

interface Props {
  card: PaymentMethod;
  selected: boolean;
  onClick?: () => void;
  className?: string;
}

export function SavedCardOption({ card, selected, onClick, className }: Props) {
  const tileClass = `rounded-lg border p-3 text-left transition-colors ${
    selected
      ? "border-primary bg-primary/5"
      : "hover:border-muted-foreground/50"
  } ${className ?? ""}`;

  const content = (
    <div className="flex items-center gap-2">
      <CreditCard className="text-muted-foreground size-4 shrink-0" />
      <div>
        <p className="text-sm font-medium capitalize">
          {card.brand} •••• {card.last4}
        </p>
        <p className="text-muted-foreground text-xs">
          Expires {card.expMonth}/{card.expYear}
        </p>
      </div>
    </div>
  );

  return onClick ? (
    <button onClick={onClick} className={tileClass}>
      {content}
    </button>
  ) : (
    <div className={tileClass}>{content}</div>
  );
}
