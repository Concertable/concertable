import { MapPin, Search, CalendarIcon } from "lucide-react";
import type { HeaderType } from "@/types/header";
import { useSearchParams } from "@/hooks/useSearchParams";
import { LocationPicker } from "@/components/LocationPicker";
import { DateRangePicker } from "@/components/DateRangePicker";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";

export interface SearchFilters {
  query?: string;
  headerType: HeaderType;
  location?: LatLng;
  from?: string;
  to?: string;
}

interface Props {
  onSearch: (filters: SearchFilters) => void;
  defaultHeaderType?: HeaderType;
}

export function SearchBar({ onSearch, defaultHeaderType = "concert" }: Props) {
  const { query, setQuery, headerType, location, setLocation, from, to, setDates } = useSearchParams(defaultHeaderType);

  function handleSearch() {
    onSearch({ query, headerType, location, from, to });
  }

  return (
    <div className="flex items-stretch w-full bg-background rounded-full shadow-md overflow-visible border border-border">
      <div className="flex items-center gap-2 px-4 py-3 min-w-48">
        <MapPin className="text-muted-foreground shrink-0" size={18} />
        <LocationPicker onSelect={setLocation} />
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
