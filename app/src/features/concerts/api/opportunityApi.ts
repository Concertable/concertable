import api from "@/lib/axios";
import type { Pagination } from "@/types/common";
import type { PaginationParams } from "@/hooks/usePagination";
import type { Opportunity } from "../types";

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
