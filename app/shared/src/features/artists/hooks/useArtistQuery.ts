import { useQuery } from "@tanstack/react-query";
import artistApi from "../api/artistApi";

export const artistKeys = {
  all: () => ["artist"] as const,
  details: () => ["artist", "details"] as const,
  byId: (id: number) => ["artist", id] as const,
};

export function useArtistQuery() {
  return useQuery({
    queryKey: artistKeys.details(),
    queryFn: artistApi.getArtist,
  });
}

export function useArtistByIdQuery(id: number) {
  return useQuery({
    queryKey: artistKeys.byId(id),
    queryFn: () => artistApi.getArtistById(id),
  });
}
