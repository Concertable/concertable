import { NewCardSection } from "@/components/NewCardSection";

interface Props {
  selected: boolean;
  onClick: () => void;
  onConfirmed: (paymentMethodId: string) => void;
  fullWidth?: boolean;
}

export function NewCardOption({
  selected,
  onClick,
  onConfirmed,
  fullWidth,
}: Props) {
  return (
    <>
      <button
        onClick={onClick}
        className={`rounded-lg border p-3 text-left transition-colors ${
          selected
            ? "border-primary bg-primary/5"
            : "hover:border-muted-foreground/50"
        } ${fullWidth ? "col-span-2" : ""}`}
      >
        <p className="text-sm font-medium">New card</p>
        <p className="text-muted-foreground text-xs">Enter card details</p>
      </button>
      {selected && (
        <div className="col-span-2">
          <NewCardSection onConfirmed={onConfirmed} />
        </div>
      )}
    </>
  );
}
