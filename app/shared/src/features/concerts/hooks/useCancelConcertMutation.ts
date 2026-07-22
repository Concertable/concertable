import { useMutation, useQueryClient } from "@tanstack/react-query";
import concertApi from "../api/concertApi";
import { concertKeys } from "./useConcertQuery";

export function useCancelConcertMutation(id: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => concertApi.cancelConcert(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: concertKeys.byId(id) });
      queryClient.invalidateQueries({ queryKey: concertKeys.my(id) });
    },
  });
}
