import dayjs from "dayjs";
import type { DateRange } from "react-day-picker";

export function useMultiDayDateRange(
  startDate: string,
  endDate: string,
  onChange: (start: string, end: string) => void,
) {
  const start = dayjs(startDate);
  const end = dayjs(endDate);

  return {
    range: { from: start.toDate(), to: end.toDate() } satisfies DateRange,
    startTime: start.format("HH:mm"),
    endTime: end.format("HH:mm"),
    setRange(value: DateRange | undefined) {
      if (!value?.from) return;
      const nextStart = dayjs(value.from)
        .hour(start.hour())
        .minute(start.minute())
        .second(0)
        .millisecond(0);
      const baseEnd = value.to ? dayjs(value.to) : nextStart;
      const nextEnd = baseEnd
        .hour(end.hour())
        .minute(end.minute())
        .second(0)
        .millisecond(0);
      onChange(nextStart.toISOString(), nextEnd.toISOString());
    },
    setStartTime(value: string) {
      const [h, m] = value.split(":").map(Number);
      onChange(
        start.hour(h).minute(m).second(0).millisecond(0).toISOString(),
        endDate,
      );
    },
    setEndTime(value: string) {
      const [h, m] = value.split(":").map(Number);
      onChange(
        startDate,
        end.hour(h).minute(m).second(0).millisecond(0).toISOString(),
      );
    },
  };
}
