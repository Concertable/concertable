import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useMyArtistQuery } from "./useArtistQuery";
import { useArtistStore } from "../store/useArtistStore";
import artistApi from "../api/artistApi";

export function useMyArtist() {
  const query = useMyArtistQuery();
  const queryClient = useQueryClient();

  const { toggleEdit, resetDraft, draft, banner, avatar, isDirty, editMode } =
    useArtistStore();

  const mutation = useMutation({
    mutationFn: () => artistApi.updateArtist(draft!, banner, avatar),
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
