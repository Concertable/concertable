import { useQuery } from "@tanstack/react-query";
import { getConcert } from "@/api/concertApi";

export function useConcertQuery(id: number) {
  return useQuery({
    queryKey: ["concert", id],
    queryFn: () => getConcert(id),
  });
}
