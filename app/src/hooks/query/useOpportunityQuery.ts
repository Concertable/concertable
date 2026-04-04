import { useQuery, keepPreviousData } from "@tanstack/react-query";
import * as opportunityApi from "@/api/opportunityApi";
import type { PaginationParams } from "@/hooks/usePagination";

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
