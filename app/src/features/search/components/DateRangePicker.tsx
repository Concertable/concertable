import { useState } from "react";
import dayjs from "dayjs";
import type { DateRange } from "react-day-picker";
import { Calendar } from "@/components/ui/calendar";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";

interface Props {
  onChange?: (from: string | undefined, to: string | undefined) => void;
}

export function DateRangePicker({ onChange }: Readonly<Props>) {
  const [range, setRange] = useState<DateRange | undefined>();

  function handleSelect(value: DateRange | undefined) {
    setRange(value);
    onChange?.(
      value?.from ? dayjs(value.from).format("YYYY-MM-DD") : undefined,
      value?.to ? dayjs(value.to).format("YYYY-MM-DD") : undefined,
    );
  }

  return (
    <Popover>
      <PopoverTrigger asChild>
        <span className="text-foreground cursor-pointer text-sm">
          {range?.from ? (
            `${dayjs(range.from).format("DD/MM/YYYY")}${range.to ? ` – ${dayjs(range.to).format("DD/MM/YYYY")}` : ""}`
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
