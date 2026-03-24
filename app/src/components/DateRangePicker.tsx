import { useState } from "react";
import { format } from "date-fns";
import type { DateRange } from "react-day-picker";
import { Calendar } from "@/components/ui/calendar";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { Button } from "@/components/ui/button";

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
        <Button variant="outline">
          {range?.from ? (
            range.to ? (
              `${format(range.from, "dd MMM")} – ${format(range.to, "dd MMM")}`
            ) : (
              format(range.from, "dd MMM yyyy")
            )
          ) : (
            "Date"
          )}
        </Button>
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
