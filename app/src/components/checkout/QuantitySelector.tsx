import { Minus, Plus } from "lucide-react";
import { Button } from "@/components/ui/button";

interface Props {
  value: number;
  onChange: (next: number) => void;
  min?: number;
  max: number;
  disabled?: boolean;
}

export function QuantitySelector({
  value,
  onChange,
  min = 1,
  max,
  disabled,
}: Props) {
  return (
    <div className="flex items-center gap-2">
      <Button
        variant="outline"
        size="icon-xs"
        className="size-4 rounded p-0 [&_svg]:size-2.5"
        onClick={() => onChange(Math.max(min, value - 1))}
        disabled={value <= min || disabled}
      >
        <Minus />
      </Button>
      <span className="w-4 text-center text-xs font-medium tabular-nums">
        {value}
      </span>
      <Button
        variant="outline"
        size="icon-xs"
        className="size-4 rounded p-0 [&_svg]:size-2.5"
        onClick={() => onChange(Math.min(max, value + 1))}
        disabled={value >= max || disabled}
      >
        <Plus />
      </Button>
    </div>
  );
}
