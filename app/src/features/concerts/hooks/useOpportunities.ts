import { useQueryClient } from "@tanstack/react-query";
import opportunityApi from "../api/opportunityApi";
import { useOpportunitiesStore } from "../store/useOpportunitiesStore";
import {
  opportunitiesQueryKey,
  useAllOpportunitiesQuery,
} from "./useOpportunitiesQuery";

export function useOpportunities(venueId: number) {
  const enabled = venueId > 0;
  const { isLoading, isError } = useAllOpportunitiesQuery(venueId, enabled);
  const queryClient = useQueryClient();
  const queryKey = opportunitiesQueryKey(venueId);

  const opportunities = useOpportunitiesStore((s) => s.opportunities);
  const isDirty = useOpportunitiesStore((s) => s.isDirty);
  const hydrate = useOpportunitiesStore((s) => s.hydrate);
  const reset = useOpportunitiesStore((s) => s.reset);
  const add = useOpportunitiesStore((s) => s.add);
  const remove = useOpportunitiesStore((s) => s.remove);

  return {
    opportunities,
    isLoading,
    isError,
    isDirty,
    hydrate,
    reset,
    add,
    remove,
    save: async () => {
      await opportunityApi.update(venueId, opportunities);
      await queryClient.invalidateQueries({ queryKey });
    },
  };
}
