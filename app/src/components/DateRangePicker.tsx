import { useState } from "react";
import { format } from "date-fns";
import type { DateRange } from "react-day-picker";
import { Calendar } from "@/components/ui/calendar";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";

interface Props {
  onChange?: (range: DateRange | undefined) => void;
}

export function DateRangePicker({ onChange }: Props) {
  const [range, setRange] = useState<DateRange | undefined>();

  function handleSelect(value: DateRange | undefined) {
    setRange(value);
    onChange?.(value);
  }

  return (
    <Popover>
      <PopoverTrigger asChild>
        <span className="text-sm cursor-pointer text-foreground">
          {range?.from ? (
            range.to ? (
              `${format(range.from, "dd MMM")} – ${format(range.to, "dd MMM")}`
            ) : (
              format(range.from, "dd MMM yyyy")
            )
          ) : (
            <span className="text-muted-foreground">Date</span>
          )}
        </span>
      </PopoverTrigger>
      <PopoverContent className="w-auto p-0" align="start">
        <Calendar
          mode="range"
          selected={range}
          onSelect={handleSelect}
          numberOfMonths={2}
        />
      </PopoverContent>
    </Popover>
  );
}
