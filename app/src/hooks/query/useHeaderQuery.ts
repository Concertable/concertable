import { useQuery } from "@tanstack/react-query";
import { searchHeaders } from "@/api/headerApi";
import type { SearchFilters } from "@/components/SearchBar";
import type { Header } from "@/types/header";
import type { Pagination } from "@/types/common";

export function useHeaderQuery(filters: SearchFilters) {
  return useQuery<Pagination<Header>>({
    queryKey: ["headers", filters],
    queryFn: () => searchHeaders(filters),
    enabled: !!filters.headerType,
  });
}
