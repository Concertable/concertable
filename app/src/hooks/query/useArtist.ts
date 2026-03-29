import { useQuery } from "@tanstack/react-query";
import { getArtist, getMyArtist } from "@/api/artistApi";

export function useArtistQuery(id: number) {
  return useQuery({
    queryKey: ["artist", id],
    queryFn: () => getArtist(id),
  });
}

export function useMyArtistQuery() {
  return useQuery({
    queryKey: ["artist", "my"],
    queryFn: getMyArtist,
  });
}
