import type { SearchFilters } from "@/components/SearchBar";
import type { HeaderType } from "@/types/header";

export function validateSearchFilters(defaultHeaderType: HeaderType) {
  return (search: Record<string, unknown>): SearchFilters => ({
    query: (search.query as string) ?? undefined,
    headerType: (search.headerType as HeaderType) ?? defaultHeaderType,
    location: search.location as SearchFilters["location"],
    from: (search.from as string) ?? undefined,
    to: (search.to as string) ?? undefined,
  });
}
