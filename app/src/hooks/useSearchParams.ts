import { useState } from "react";
import { useSearch } from "@tanstack/react-router";
import type { LatLng } from "@/types/location";
import type { HeaderType } from "@/types/header";
import type { SearchFilters } from "@/components/SearchBar";

export function useSearchParams(defaultHeaderType: HeaderType) {
  const search = useSearch({ strict: false }) as Partial<SearchFilters>;

  const [query, setQuery] = useState(search.query);
  const [location, setLocation] = useState<LatLng | undefined>(search.location);
  const [from, setFrom] = useState(search.from);
  const [to, setTo] = useState(search.to);
  const headerType: HeaderType = search.headerType ?? defaultHeaderType;

  function setDates(newFrom: string | undefined, newTo: string | undefined) {
    setFrom(newFrom);
    setTo(newTo);
  }

  return { query, setQuery, headerType, location, setLocation, from, to, setDates };
}
