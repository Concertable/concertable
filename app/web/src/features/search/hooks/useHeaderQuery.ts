import { useQuery } from "@tanstack/react-query";
import type { Pagination } from "@/types/common";
import headerApi from "../api/headerApi";
import type { Header, HeaderType } from "../types";
import type { SearchFilters } from "../schemas/searchSchema";

export function useHeaderQuery(filters: SearchFilters) {
  return useQuery<Pagination<Header>>({
    queryKey: ["headers", filters],
    queryFn: () => headerApi.searchHeaders(filters),
    enabled: !!filters.headerType,
  });
}

export function useHeaderAmountQuery(headerType: HeaderType, amount = 15) {
  return useQuery<Header[]>({
    queryKey: ["headers", headerType, amount],
    queryFn: () => headerApi.getByAmount(amount, headerType),
  });
}
