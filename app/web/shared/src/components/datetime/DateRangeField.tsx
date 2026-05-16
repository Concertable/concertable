import { useState } from "react";
import dayjs from "dayjs";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import { SingleDayPicker } from "./SingleDayPicker";
import { MultiDayPicker } from "./MultiDayPicker";

interface Props {
  startDate: string;
  endDate: string;
  onChange: (start: string, end: string) => void;
}

export function DateRangeField({
  startDate,
  endDate,
  onChange,
}: Readonly<Props>) {
  const [singleDay, setSingleDay] = useState(
    () => dayjs(endDate).diff(dayjs(startDate), "hour") <= 24,
  );

  return (
    <div className="space-y-2">
      <div className="flex items-center justify-between">
        <Label className="text-muted-foreground text-xs">When</Label>
        <label
          className="flex cursor-pointer items-center gap-1.5 text-xs"
          data-testid="datetime-single-day-toggle"
        >
          <Checkbox
            checked={singleDay}
            onCheckedChange={(v) => setSingleDay(v === true)}
          />
          <span className="text-muted-foreground">Single day</span>
        </label>
      </div>

      {singleDay ? (
        <SingleDayPicker
          startDate={startDate}
          endDate={endDate}
          onChange={onChange}
        />
      ) : (
        <MultiDayPicker
          startDate={startDate}
          endDate={endDate}
          onChange={onChange}
        />
      )}
    </div>
  );
}
