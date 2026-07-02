import { useMutation, useQueryClient } from "@tanstack/react-query";
import concertApi from "../api/concertApi";

export function useCancelConcert(id: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => concertApi.cancelConcert(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["concert", id] });
    },
  });
}
