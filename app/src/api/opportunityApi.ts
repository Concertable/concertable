import api from "@/lib/axios";
import type { Opportunity } from "@/types/opportunity";
import type { Pagination } from "@/types/common";
import type { PaginationParams } from "@/hooks/usePagination";

export async function getOpportunitiesByVenueId(venueId: number, params: PaginationParams): Promise<Pagination<Opportunity>> {
  const { data } = await api.get<Pagination<Opportunity>>(`/concertopportunity/active/venue/${venueId}`, { params });
  return data;
}
