import { useState } from "react";
import { MapPin, Search, CalendarIcon } from "lucide-react";
import { useApiIsLoaded } from "@vis.gl/react-google-maps";
import type { HeaderType } from "@/types/header";
import { useSearchFiltersStore } from "@/store/useSearchFiltersStore";
import { LocationPicker } from "@/components/LocationPicker";
import { DateRangePicker } from "@/components/DateRangePicker";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";

export interface SearchFilters {
  query?: string;
  headerType: HeaderType;
  lat?: number;
  lng?: number;
  from?: string;
  to?: string;
}

interface Props {
  onSearch: (filters: SearchFilters) => void;
  defaultHeaderType?: HeaderType;
}

export function SearchBar({ onSearch, defaultHeaderType = "concert" }: Readonly<Props>) {
  const mapsLoaded = useApiIsLoaded();
  const storeFilters = useSearchFiltersStore((s) => s.filters);

  const [query, setQuery] = useState(storeFilters.query);
  const [lat, setLat] = useState(storeFilters.lat);
  const [lng, setLng] = useState(storeFilters.lng);
  const [from, setFrom] = useState(storeFilters.from);
  const [to, setTo] = useState(storeFilters.to);
  const headerType: HeaderType = storeFilters.headerType ?? defaultHeaderType;

  function setLocation(newLat: number, newLng: number) {
    setLat(newLat);
    setLng(newLng);
  }

  function setDates(newFrom: string | undefined, newTo: string | undefined) {
    setFrom(newFrom);
    setTo(newTo);
  }

  function handleSearch() {
    onSearch({ query, headerType, lat, lng, from, to });
  }

  return (
    <div className="flex items-stretch w-full bg-background rounded-full shadow-md overflow-visible border border-border">
      <div className="flex items-center gap-2 px-4 py-3 min-w-48">
        <MapPin className="text-muted-foreground shrink-0" size={18} />
        {mapsLoaded && (
          <LocationPicker
            onSelect={setLocation}
            latLng={lat && lng ? { lat, lng } : undefined}
          />
        )}
      </div>

      <Separator orientation="vertical" />

      <input
        value={query ?? ""}
        onChange={(e) => setQuery(e.target.value)}
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
