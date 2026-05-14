import dayjs from "dayjs";
import { Calendar } from "@/components/ui/calendar";
import { Input } from "@/components/ui/input";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { Button } from "@/components/ui/button";
import { useMultiDayDateRange } from "./useMultiDayDateRange";

interface Props {
  startDate: string;
  endDate: string;
  onChange: (start: string, end: string) => void;
}

export function MultiDayPicker({
  startDate,
  endDate,
  onChange,
}: Readonly<Props>) {
  const { range, startTime, endTime, setRange, setStartTime, setEndTime } =
    useMultiDayDateRange(startDate, endDate, onChange);

  return (
    <div className="space-y-2">
      <Popover>
        <PopoverTrigger asChild>
          <Button
            variant="outline"
            size="sm"
            data-testid="multi-day-range"
            className="justify-start"
          >
            {dayjs(range.from).format("D MMM YYYY")} —{" "}
            {dayjs(range.to).format("D MMM YYYY")}
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-auto p-0" align="start">
          <Calendar
            mode="range"
            selected={range}
            numberOfMonths={2}
            onSelect={setRange}
          />
        </PopoverContent>
      </Popover>

      <div className="grid grid-cols-2 gap-2">
        <Input
          type="time"
          value={startTime}
          onChange={(e) => setStartTime(e.target.value)}
          data-testid="multi-day-start-time"
        />
        <Input
          type="time"
          value={endTime}
          onChange={(e) => setEndTime(e.target.value)}
          data-testid="multi-day-end-time"
        />
      </div>
    </div>
  );
}
