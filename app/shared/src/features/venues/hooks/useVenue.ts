import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useVenueQuery, venueKeys } from "./useVenueQuery";
import { useVenueStore } from "../store/useVenueStore";
import venueApi from "../api/venueApi";
import type { Venue } from "../types";

export interface UseVenueOptions {
  onSuccess?: (saved: Venue) => void;
  onError?: (err: unknown) => void;
  afterSave?: () => Promise<void>;
  onToggleEdit?: () => void;
  onResetDraft?: () => void;
  extraDirty?: boolean;
}

export interface UseVenueResult {
  venue: Venue | undefined;
  draft: Venue | undefined;
  isLoading: boolean;
  isError: boolean;
  editMode: boolean;
  isDirty: boolean;
  isSaving: boolean;
  save: () => void;
  toggleEdit: () => void;
  resetDraft: () => void;
}

export function useVenue(options?: UseVenueOptions): UseVenueResult {
  const query = useVenueQuery();
  const queryClient = useQueryClient();

  const {
    beginEdit,
    endEdit,
    draft,
    banner,
    avatar,
    isDirty: venueIsDirty,
    editMode,
  } = useVenueStore();

  const mutation = useMutation({
    mutationFn: async () => {
      const saved = await venueApi.updateVenue(draft!, banner, avatar);
      if (options?.afterSave) await options.afterSave();
      return saved;
    },
    onSuccess: (saved) => {
      queryClient.setQueryData(venueKeys.details(), saved);
      queryClient.setQueryData(venueKeys.byId(saved.id), saved);
      endEdit();
      options?.onSuccess?.(saved);
    },
    onError: (err) => options?.onError?.(err),
  });

  return {
    venue: query.data ?? undefined,
    draft,
    isLoading: query.isLoading,
    isError: query.isError,
    editMode,
    isDirty: venueIsDirty || (options?.extraDirty ?? false),
    save: mutation.mutate,
    isSaving: mutation.isPending,
    toggleEdit: () => {
      if (editMode) endEdit();
      else beginEdit(query.data!);
      options?.onToggleEdit?.();
    },
    resetDraft: () => {
      endEdit();
      options?.onResetDraft?.();
    },
  };
}
