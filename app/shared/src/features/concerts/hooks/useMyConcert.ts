import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useConcert } from "./useConcert";
import { useConcertStore } from "../store/useConcertStore";
import concertApi from "../api/concertApi";
import type { Concert } from "../types";
import type { UseConcertResult } from "./useConcert";

interface UseMyConcertResult extends UseConcertResult {
  draft: Concert | undefined;
  editMode: boolean;
  isDirty: boolean;
  isSaving: boolean;
  save: () => void;
  toggleEdit: () => void;
  resetDraft: () => void;
}

export function useMyConcert(id: number): UseMyConcertResult {
  const { concert, isLoading, isError } = useConcert(id);
  const queryClient = useQueryClient();

  const { toggleEdit, resetDraft, draft, isDirty, editMode } =
    useConcertStore();

  const mutation = useMutation({
    mutationFn: () =>
      concertApi.updateConcert(id, {
        name: draft!.name,
        about: draft!.about,
        price: draft!.price,
        totalTickets: draft!.totalTickets,
      }),
    onSuccess: (saved) => {
      queryClient.setQueryData(["concert", id], saved);
      resetDraft(saved);
    },
  });

  return {
    concert,
    draft,
    isLoading,
    isError,
    editMode,
    isDirty,
    save: mutation.mutate,
    isSaving: mutation.isPending,
    toggleEdit: () => toggleEdit(concert!),
    resetDraft: () => resetDraft(concert!),
  };
}
