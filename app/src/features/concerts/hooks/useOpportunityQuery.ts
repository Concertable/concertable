import { useQuery, keepPreviousData } from "@tanstack/react-query";
import type { PaginationParams } from "@/hooks/usePagination";
import opportunityApi from "../api/opportunityApi";

export function useOpportunitiesByVenueQuery(
  venueId: number,
  params: PaginationParams,
) {
  return useQuery({
    queryKey: ["opportunities", "venue", venueId, params],
    queryFn: () => opportunityApi.getOpportunitiesByVenueId(venueId, params),
    placeholderData: keepPreviousData,
  });
}
