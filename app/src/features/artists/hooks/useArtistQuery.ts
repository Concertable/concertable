import { useQuery } from "@tanstack/react-query";
import artistApi from "../api/artistApi";

export function useArtistQuery(id: number) {
  return useQuery({
    queryKey: ["artist", id],
    queryFn: () => artistApi.getArtist(id),
  });
}

export function useMyArtistQuery() {
  return useQuery({
    queryKey: ["artist", "my"],
    queryFn: artistApi.getMyArtist,
  });
}
