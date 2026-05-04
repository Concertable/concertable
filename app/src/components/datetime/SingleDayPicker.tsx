import dayjs from "dayjs";
import { Calendar } from "@/components/ui/calendar";
import { Input } from "@/components/ui/input";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { Button } from "@/components/ui/button";
import { useSingleDayDateRange } from "./useSingleDayDateRange";

interface Props {
  startDate: string;
  endDate: string;
  onChange: (start: string, end: string) => void;
}

export function SingleDayPicker({
  startDate,
  endDate,
  onChange,
}: Readonly<Props>) {
  const { date, startTime, endTime, setDate, setStartTime, setEndTime } =
    useSingleDayDateRange(startDate, endDate, onChange);

  return (
    <div className="grid grid-cols-3 gap-2">
      <Popover>
        <PopoverTrigger asChild>
          <Button
            variant="outline"
            size="sm"
            data-testid="single-day-date"
            className="justify-start"
          >
            {dayjs(date).format("D MMM YYYY")}
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-auto p-0" align="start">
          <Calendar mode="single" selected={date} onSelect={setDate} />
        </PopoverContent>
      </Popover>

      <Input
        type="time"
        value={startTime}
        onChange={(e) => setStartTime(e.target.value)}
        data-testid="single-day-start-time"
      />

      <Input
        type="time"
        value={endTime}
        onChange={(e) => setEndTime(e.target.value)}
        data-testid="single-day-end-time"
      />
    </div>
  );
}
