import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useArtistQuery, artistKeys } from "./useArtistQuery";
import { useArtistStore } from "../store/useArtistStore";
import artistApi from "../api/artistApi";
import type { Artist } from "../types";

export interface UseArtistOptions {
  onSuccess?: (saved: Artist) => void;
  onError?: (err: unknown) => void;
}

export interface UseArtistResult {
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

export function useArtist(options?: UseArtistOptions): UseArtistResult {
  const query = useArtistQuery();
  const queryClient = useQueryClient();

  const { beginEdit, endEdit, draft, banner, avatar, isDirty, editMode } =
    useArtistStore();

  const mutation = useMutation({
    mutationFn: () => artistApi.updateArtist(draft!, banner, avatar),
    onSuccess: (saved) => {
      queryClient.setQueryData(artistKeys.details(), saved);
      queryClient.setQueryData(artistKeys.byId(saved.id), saved);
      endEdit();
      options?.onSuccess?.(saved);
    },
    onError: (err) => options?.onError?.(err),
  });

  return {
    artist: query.data ?? undefined,
    draft,
    isLoading: query.isLoading,
    isError: query.isError,
    editMode,
    isDirty,
    save: mutation.mutate,
    isSaving: mutation.isPending,
    toggleEdit: () => (editMode ? endEdit() : beginEdit(query.data!)),
    resetDraft: endEdit,
  };
}
