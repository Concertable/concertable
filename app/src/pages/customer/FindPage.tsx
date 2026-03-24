import { Input } from "@/components/ui/input";
import { LocationPicker } from "@/components/LocationPicker";
import type { LatLng } from "@/types/location";
import { DateRangePicker } from "@/components/DateRangePicker";
import type { DateRange } from "react-day-picker";

export default function FindPage() {
  function handleLocationSelect(location: LatLng) {
    console.log(location);
  }

  function handleDateChange(range: DateRange | undefined) {
    console.log(range);
  }

  return (
    <div className="p-6 flex gap-2">
      <LocationPicker onSelect={handleLocationSelect} />
      <Input placeholder="Search" />
      <DateRangePicker onChange={handleDateChange} />
    </div>
  );
}
