import { MapPin, Search, CalendarIcon } from "lucide-react";
import { useApiIsLoaded } from "@vis.gl/react-google-maps";
import { useSearchFiltersStore } from "@/store/useSearchFiltersStore";
import { useSearchFilters } from "@/hooks/useSearchFilters";
import { LocationPicker } from "@/components/LocationPicker";
import { DateRangePicker } from "@/components/DateRangePicker";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";

export function SearchBar() {
  const mapsLoaded = useApiIsLoaded();
  const { filters, setFilters } = useSearchFiltersStore();
  const { updateFilters } = useSearchFilters();

  function setLocation(newLat: number, newLng: number) {
    setFilters({ ...filters, lat: newLat, lng: newLng });
  }

  function setDates(newFrom: string | undefined, newTo: string | undefined) {
    setFilters({ ...filters, from: newFrom, to: newTo });
  }

  function handleSearch() {
    updateFilters(useSearchFiltersStore.getState().filters);
  }

  return (
    <div className="bg-background border-border flex w-full items-stretch overflow-visible rounded-full border shadow-md">
      <div className="flex flex-1 items-center gap-2 px-4 py-3">
        <MapPin className="text-muted-foreground shrink-0" size={18} />
        {mapsLoaded && (
          <LocationPicker
            onSelect={setLocation}
            latLng={
              filters.lat && filters.lng
                ? { lat: filters.lat, lng: filters.lng }
                : undefined
            }
          />
        )}
      </div>

      <Separator orientation="vertical" />

      <input
        value={filters.query ?? ""}
        onChange={(e) => setFilters({ ...filters, query: e.target.value })}
        onKeyDown={(e) => e.key === "Enter" && handleSearch()}
        placeholder="Search"
        className="text-foreground placeholder:text-muted-foreground flex-1 bg-transparent px-4 py-3 text-sm outline-none"
      />

      <Separator orientation="vertical" />

      <div className="flex flex-1 items-center gap-2 px-4 py-3">
        <CalendarIcon className="text-muted-foreground shrink-0" size={18} />
        <DateRangePicker onChange={setDates} />
      </div>

      <div className="flex items-center pr-2">
        <Button onClick={handleSearch} size="icon" className="rounded-full">
          <Search size={16} />
        </Button>
      </div>
    </div>
  );
}
