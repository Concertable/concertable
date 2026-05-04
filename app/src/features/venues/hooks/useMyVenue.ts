import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useMyVenueQuery } from "./useVenueQuery";
import { useVenueStore } from "../store/useVenueStore";
import venueApi from "../api/venueApi";
import opportunityApi from "@/features/concerts/api/opportunityApi";
import type { Venue } from "../types";
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
    toggleEdit,
    resetDraft,
    draft,
    banner,
    avatar,
    isDirty,
    editMode,
    opportunities,
  } = useVenueStore();

  const mutation = useMutation({
    mutationFn: async () => {
      const saved = await venueApi.updateVenue(draft!, banner, avatar);
      await opportunityApi.update(saved.id, opportunities);
      return saved;
    },
    onSuccess: (saved) => {
      queryClient.setQueryData(["venue", "my"], saved);
      queryClient.invalidateQueries({
        queryKey: ["opportunities", "venue", saved.id],
      });
      resetDraft(saved);
    },
  });

  return {
    venue: query.data,
    draft,
    isLoading: query.isLoading,
    isError: query.isError,
    editMode,
    isDirty,
    save: mutation.mutate,
    isSaving: mutation.isPending,
    toggleEdit: () => toggleEdit(query.data!),
    resetDraft: () => resetDraft(query.data!),
  };
}
