import api from "@/lib/axios";
import type { Pagination } from "@/types/common";
import type { PaginationParams } from "@/hooks/usePagination";
import type { Opportunity, OpportunityDraft } from "../types";

const opportunityApi = {
  getPaged: async (
    venueId: number,
    params: PaginationParams,
  ): Promise<Pagination<Opportunity>> => {
    const { data } = await api.get<Pagination<Opportunity>>(
      `/opportunity/active/venue/${venueId}`,
      { params },
    );
    return data;
  },

  getAll: async (venueId: number): Promise<Opportunity[]> => {
    const { data } = await api.get<Opportunity[]>(
      `/venue/${venueId}/opportunities`,
    );
    return data;
  },

  update: async (
    venueId: number,
    desired: (Opportunity | OpportunityDraft)[],
  ): Promise<void> => {
    await api.put(
      `/venue/${venueId}/opportunities`,
      desired.map((o) => ({
        id: "id" in o ? o.id : undefined,
        startDate: o.startDate,
        endDate: o.endDate,
        genreIds: o.genres.map((g) => g.id),
        contract: o.contract,
      })),
    );
  },
};

export default opportunityApi;
