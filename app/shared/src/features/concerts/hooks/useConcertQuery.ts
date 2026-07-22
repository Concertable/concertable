import { useQuery } from "@tanstack/react-query";
import concertApi from "../api/concertApi";

export const concertKeys = {
  all: () => ["concert"] as const,
  byId: (id: number) => ["concert", id] as const,
  // The owner read carries action links the public read doesn't, so the two must not
  // share a cache entry within a manager app that browses both.
  my: (id: number) => ["concert", "my", id] as const,
};

export function useConcertQuery(id: number) {
  return useQuery({
    queryKey: concertKeys.byId(id),
    queryFn: () => concertApi.getConcert(id),
  });
}
