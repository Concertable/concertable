import api from "@/lib/axios";
import type { Opportunity } from "@/types/opportunity";
import type { Pagination } from "@/types/common";
import type { PaginationParams } from "@/hooks/usePagination";

const opportunityApi = {
  getOpportunitiesByVenueId: async (
    venueId: number,
    params: PaginationParams,
  ): Promise<Pagination<Opportunity>> => {
    const { data } = await api.get<Pagination<Opportunity>>(
      `/opportunity/active/venue/${venueId}`,
      { params },
    );
    return data;
  },
};

export default opportunityApi;
