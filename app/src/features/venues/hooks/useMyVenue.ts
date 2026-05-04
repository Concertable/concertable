import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useMyVenueQuery } from "./useVenueQuery";
import { useVenueStore } from "../store/useVenueStore";
import venueApi from "../api/venueApi";
import { useOpportunities } from "@/features/concerts/hooks/useOpportunities";
import { opportunitiesQueryKey } from "@/features/concerts/hooks/useOpportunitiesQuery";
import type { Venue } from "../types";
import type { Opportunity } from "@/features/concerts/types";
import type { UseVenueResult } from "./useVenue";

interface UseMyVenueResult extends UseVenueResult {
  draft: Venue | undefined;
  editMode: boolean;
  isDirty: boolean;
  isSaving: boolean;
  save: () => void;
  toggleEdit: () => void;
  resetDraft: () => void;
}

export function useMyVenue(): UseMyVenueResult {
  const query = useMyVenueQuery();
  const queryClient = useQueryClient();

  const {
    toggleEdit: storeToggleEdit,
    resetDraft: storeResetDraft,
    draft,
    banner,
    avatar,
    isDirty: venueIsDirty,
    editMode,
  } = useVenueStore();

  const venueId = query.data?.id ?? 0;
  const {
    save: saveOpportunities,
    hydrate: hydrateOpportunities,
    reset: resetOpportunities,
    isDirty: opportunitiesIsDirty,
  } = useOpportunities(venueId);

  const mutation = useMutation({
    mutationFn: async () => {
      const saved = await venueApi.updateVenue(draft!, banner, avatar);
      await saveOpportunities();
      return saved;
    },
    onSuccess: (saved) => {
      queryClient.setQueryData(["venue", "my"], saved);
      storeResetDraft(saved);
      resetOpportunities();
    },
  });

  return {
    venue: query.data,
    draft,
    isLoading: query.isLoading,
    isError: query.isError,
    editMode,
    isDirty: venueIsDirty || opportunitiesIsDirty,
    save: mutation.mutate,
    isSaving: mutation.isPending,
    toggleEdit: () => {
      const cached =
        queryClient.getQueryData<Opportunity[]>(
          opportunitiesQueryKey(venueId),
        ) ?? [];
      storeToggleEdit(query.data!);
      hydrateOpportunities(cached);
    },
    resetDraft: () => {
      storeResetDraft(query.data!);
      resetOpportunities();
    },
  };
}
