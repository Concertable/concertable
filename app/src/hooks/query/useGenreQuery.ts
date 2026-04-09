import { useQuery } from "@tanstack/react-query";
import genreApi from "@/api/genreApi";

export function useGenresQuery() {
  return useQuery({
    queryKey: ["genres"],
    queryFn: genreApi.getAll,
  });
}
