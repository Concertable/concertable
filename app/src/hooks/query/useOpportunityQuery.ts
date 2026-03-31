import { useQuery, keepPreviousData } from "@tanstack/react-query";
import { getOpportunitiesByVenueId } from "@/api/opportunityApi";
import type { PaginationParams } from "@/hooks/usePagination";

export function useOpportunitiesByVenueQuery(venueId: number, params: PaginationParams) {
  return useQuery({
    queryKey: ["opportunities", "venue", venueId, params],
    queryFn: () => getOpportunitiesByVenueId(venueId, params),
    placeholderData: keepPreviousData,
  });
}
