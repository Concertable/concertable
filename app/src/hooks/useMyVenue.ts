import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useMyVenueQuery } from "@/hooks/query/useVenue";
import { useVenueStore } from "@/store/useVenueStore";
import { updateVenue } from "@/api/venueApi";

export function useMyVenue() {
  const query = useMyVenueQuery();
  const queryClient = useQueryClient();

  const { toggleEdit, resetDraft, draft, image, isDirty, editMode } = useVenueStore();

  const mutation = useMutation({
    mutationFn: () => updateVenue(draft!, image),
    onSuccess: (saved) => {
      queryClient.setQueryData(["venue", "my"], saved);
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
