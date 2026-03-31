import { useConcertQuery } from "@/hooks/query/useConcertQuery";
import type { Concert } from "@/types/concert";

export interface UseConcertResult {
  concert: Concert | undefined;
  isLoading: boolean;
  isError: boolean;
}

export function useConcert(id: number): UseConcertResult {
  const { data: concert, isLoading, isError } = useConcertQuery(id);
  return { concert, isLoading, isError };
}
