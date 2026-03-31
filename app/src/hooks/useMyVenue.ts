import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useMyVenueQuery } from "@/hooks/query/useVenueQuery";
import { useVenueStore } from "@/store/useVenueStore";
import { updateVenue } from "@/api/venueApi";
import type { Venue } from "@/types/venue";
import type { UseVenueResult } from "@/hooks/useVenue";

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

  const { toggleEdit, resetDraft, draft, image, isDirty, editMode } = useVenueStore();

  const mutation = useMutation({
    mutationFn: () => updateVenue(draft!, image),
    onSuccess: (saved) => {
      queryClient.setQueryData(["venue", "my"], saved);
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
