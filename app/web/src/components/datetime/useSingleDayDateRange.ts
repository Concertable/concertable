import dayjs from "dayjs";

export function useSingleDayDateRange(
  startDate: string,
  endDate: string,
  onChange: (start: string, end: string) => void,
) {
  const start = dayjs(startDate);
  const end = dayjs(endDate);

  function emit(nextStart: dayjs.Dayjs, endTimeHHmm: string) {
    const [h, m] = endTimeHHmm.split(":").map(Number);
    const sameDayEnd = nextStart
      .hour(h)
      .minute(m)
      .second(0)
      .millisecond(0);
    const nextEnd = sameDayEnd.isAfter(nextStart)
      ? sameDayEnd
      : sameDayEnd.add(1, "day");
    onChange(nextStart.toISOString(), nextEnd.toISOString());
  }

  return {
    date: start.toDate(),
    startTime: start.format("HH:mm"),
    endTime: end.format("HH:mm"),
    setDate(date: Date | undefined) {
      if (!date) return;
      const nextStart = dayjs(date)
        .hour(start.hour())
        .minute(start.minute())
        .second(0)
        .millisecond(0);
      emit(nextStart, end.format("HH:mm"));
    },
    setStartTime(value: string) {
      const [h, m] = value.split(":").map(Number);
      const nextStart = start.hour(h).minute(m).second(0).millisecond(0);
      emit(nextStart, end.format("HH:mm"));
    },
    setEndTime(value: string) {
      emit(start, value);
    },
  };
}
