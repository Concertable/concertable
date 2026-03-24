import { useState } from "react";
import type { LatLng } from "@/types/location";
import type { HeaderType } from "@/types/header";
import { Route } from "@/routes/_customer/find/index";

export function useSearchParamsState() {
  const defaults = Route.useSearch();

  const [query, setQuery] = useState<string | undefined>(defaults.query);
  const [headerType] = useState<HeaderType>(defaults.headerType ?? "concert");
  const [location, setLocation] = useState<LatLng | undefined>(defaults.location);
  const [from, setFrom] = useState<string | undefined>(defaults.from);
  const [to, setTo] = useState<string | undefined>(defaults.to);

  function setDates(newFrom: string | undefined, newTo: string | undefined) {
    setFrom(newFrom);
    setTo(newTo);
  }

  return { query, setQuery, headerType, location, setLocation, from, to, setDates };
}
