import type { SearchFilters } from "@/components/SearchBar";
import type { HeaderType } from "@/types/header";

export function validateSearchFilters(defaultHeaderType: HeaderType) {
  return (search: Record<string, unknown>): SearchFilters => ({
    query: (search.query as string) ?? undefined,
    headerType: (search.headerType as HeaderType) ?? defaultHeaderType,
    lat: (search.lat as number) ?? undefined,
    lng: (search.lng as number) ?? undefined,
    from: (search.from as string) ?? undefined,
    to: (search.to as string) ?? undefined,
  });
}
