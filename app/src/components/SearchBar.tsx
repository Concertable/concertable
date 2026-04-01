import { MapPin, Search, CalendarIcon } from "lucide-react";
import { useApiIsLoaded } from "@vis.gl/react-google-maps";
import { useSearchFiltersStore } from "@/store/useSearchFiltersStore";
import { useSearchFilters } from "@/hooks/useSearchFilters";
import type { SearchFilters } from "@/schemas/searchSchema";
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
    <div className="flex items-stretch w-full bg-background rounded-full shadow-md overflow-visible border border-border">
      <div className="flex items-center gap-2 px-4 py-3 min-w-48">
        <MapPin className="text-muted-foreground shrink-0" size={18} />
        {mapsLoaded && (
          <LocationPicker
            onSelect={setLocation}
            latLng={filters.lat && filters.lng ? { lat: filters.lat, lng: filters.lng } : undefined}
          />
        )}
      </div>

      <Separator orientation="vertical" />

      <input
        value={filters.query ?? ""}
        onChange={(e) => setFilters({ ...filters, query: e.target.value })}
        onKeyDown={(e) => e.key === "Enter" && handleSearch()}
        placeholder="Search"
        className="flex-1 px-4 py-3 text-sm bg-transparent outline-none text-foreground placeholder:text-muted-foreground"
      />

      <Separator orientation="vertical" />

      <div className="flex items-center gap-2 px-4 py-3 min-w-44">
        <CalendarIcon className="text-muted-foreground shrink-0" size={18} />
        <DateRangePicker onChange={setDates} />
      </div>

      <div className="pr-2 flex items-center">
        <Button onClick={handleSearch} size="icon" className="rounded-full">
          <Search size={16} />
        </Button>
      </div>
    </div>
  );
}
