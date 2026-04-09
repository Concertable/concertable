import { useQuery } from "@tanstack/react-query";
import concertApi from "@/api/concertApi";

export function useConcertQuery(id: number) {
  return useQuery({
    queryKey: ["concert", id],
    queryFn: () => concertApi.getConcert(id),
  });
}
