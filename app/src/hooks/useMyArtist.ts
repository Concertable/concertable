import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useMyArtistQuery } from "@/hooks/query/useArtistQuery";
import { useArtistStore } from "@/store/useArtistStore";
import { updateArtist } from "@/api/artistApi";

export function useMyArtist() {
  const query = useMyArtistQuery();
  const queryClient = useQueryClient();

  const { toggleEdit, resetDraft, draft, banner, avatar, isDirty, editMode } =
    useArtistStore();

  const mutation = useMutation({
    mutationFn: () => updateArtist(draft!, banner, avatar),
    onSuccess: (saved) => {
      queryClient.setQueryData(["artist", "my"], saved);
    },
  });

  return {
    artist: query.data,
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
