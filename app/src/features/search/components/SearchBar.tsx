import { useNavigate } from "@tanstack/react-router";
import { MapPin, Search, CalendarIcon } from "lucide-react";
import { useApiIsLoaded } from "@vis.gl/react-google-maps";
import { LocationPicker } from "@/components/LocationPicker";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import { useSearchFiltersStore } from "../store/useSearchFiltersStore";
import { useSearchFilters } from "../hooks/useSearchFilters";
import { useSearchState } from "../hooks/useSearchState";
import { DateRangePicker } from "./DateRangePicker";
import { AutocompleteDropdown } from "./AutocompleteDropdown";
import type { AutocompleteResult, HeaderType } from "../types";

export function SearchBar() {
  const mapsLoaded = useApiIsLoaded();
  const { filters, setFilters } = useSearchFiltersStore();
  const { updateFilters } = useSearchFilters();
  const navigate = useNavigate();
  const { open, close, inputProps } = useSearchState();
  const query = filters.query ?? "";

  function setLocation(newLat: number, newLng: number) {
    setFilters({ ...filters, lat: newLat, lng: newLng });
  }

  function setDates(newFrom: string | undefined, newTo: string | undefined) {
    setFilters({ ...filters, from: newFrom, to: newTo });
  }

  function handleSearch() {
    close();
    updateFilters(useSearchFiltersStore.getState().filters);
  }

  function handleSelect(result: AutocompleteResult) {
    close();
    navigate({
      to: `/find/${result.$type}/$id` as `/find/${HeaderType}/$id`,
      params: { id: result.id },
    });
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

      <div className="relative flex flex-1">
        <input
          value={query}
          onChange={(e) => setFilters({ ...filters, query: e.target.value })}
          onKeyDown={(e) => e.key === "Enter" && handleSearch()}
          {...inputProps}
          placeholder="Search"
          className="text-foreground placeholder:text-muted-foreground w-full bg-transparent px-4 py-3 text-sm outline-none"
        />
        {open && (
          <AutocompleteDropdown
            searchTerm={query}
            onSelect={handleSelect}
            headerType={filters.headerType}
            className="right-0 left-0 mt-2"
          />
        )}
      </div>

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
