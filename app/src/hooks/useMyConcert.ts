import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useConcertQuery } from "@/hooks/query/useConcertQuery";
import { useConcertStore } from "@/store/useConcertStore";
import { updateConcert } from "@/api/concertApi";
import type { Concert } from "@/types/concert";
import type { UseConcertResult } from "@/hooks/useConcert";

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
  const query = useConcertQuery(id);
  const queryClient = useQueryClient();

  const { toggleEdit, resetDraft, draft, isDirty, editMode } = useConcertStore();

  const mutation = useMutation({
    mutationFn: () => updateConcert(id, {
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
    concert: query.data,
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
