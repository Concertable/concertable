import { useQuery } from "@tanstack/react-query";
import { searchHeaders } from "@/api/headerApi";
import type { SearchFilters } from "@/components/SearchBar";

export function useHeaderQuery(filters: SearchFilters) {
  return useQuery({
    queryKey: ["headers", filters],
    queryFn: () => searchHeaders(filters),
    enabled: !!filters.headerType,
  });
}
