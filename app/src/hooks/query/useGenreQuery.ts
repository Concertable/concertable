import { useQuery } from "@tanstack/react-query";
import * as genreApi from "@/api/genreApi";

export function useGenresQuery() {
  return useQuery({
    queryKey: ["genres"],
    queryFn: genreApi.getAll,
  });
}
