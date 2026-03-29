import { useState } from "react";
import { useSearch } from "@tanstack/react-router";
import type { HeaderType } from "@/types/header";
import type { SearchFilters } from "@/components/SearchBar";

export function useSearchParams(defaultHeaderType: HeaderType) {
  const search = useSearch({ strict: false }) as Partial<SearchFilters>;

  const [query, setQuery] = useState(search.query);
  const [lat, setLat] = useState<number | undefined>(search.lat);
  const [lng, setLng] = useState<number | undefined>(search.lng);
  const [from, setFrom] = useState(search.from);
  const [to, setTo] = useState(search.to);
  const headerType: HeaderType = search.headerType ?? defaultHeaderType;

  function setLocation(lat: number, lng: number) {
    setLat(lat);
    setLng(lng);
  }

  function setDates(newFrom: string | undefined, newTo: string | undefined) {
    setFrom(newFrom);
    setTo(newTo);
  }

  return { query, setQuery, headerType, lat, lng, setLocation, from, to, setDates };
}
