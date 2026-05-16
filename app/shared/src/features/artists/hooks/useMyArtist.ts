import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useMyArtistQuery } from "./useArtistQuery";
import { useArtistStore } from "../store/useArtistStore";
import artistApi from "../api/artistApi";
import type { Artist } from "../types";

export interface UseMyArtistOptions {
  onSuccess?: (saved: Artist) => void;
  onError?: (err: unknown) => void;
}

export interface UseMyArtistResult {
  artist: Artist | undefined;
  draft: Artist | undefined;
  isLoading: boolean;
  isError: boolean;
  editMode: boolean;
  isDirty: boolean;
  isSaving: boolean;
  save: () => void;
  toggleEdit: () => void;
  resetDraft: () => void;
}

export function useMyArtist(options?: UseMyArtistOptions): UseMyArtistResult {
  const query = useMyArtistQuery();
  const queryClient = useQueryClient();

  const { toggleEdit, resetDraft, draft, banner, avatar, isDirty, editMode } =
    useArtistStore();

  const mutation = useMutation({
    mutationFn: () => artistApi.updateArtist(draft!, banner, avatar),
    onSuccess: (saved) => {
      queryClient.setQueryData(["artist", "my"], saved);
      resetDraft(saved);
      options?.onSuccess?.(saved);
    },
    onError: (err) => options?.onError?.(err),
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
