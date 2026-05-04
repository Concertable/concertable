import api from "@/lib/axios";
import type { Pagination } from "@/types/common";
import type { PaginationParams } from "@/hooks/usePagination";
import type { Opportunity, OpportunityRequest } from "../types";

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

  update: async (
    venueId: number,
    desired: OpportunityRequest[],
  ): Promise<void> => {
    await api.put(`/venue/${venueId}/opportunities`, desired);
  },
};

export default opportunityApi;
