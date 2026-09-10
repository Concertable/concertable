import { useQuery } from "@tanstack/react-query";
import artistApi from "../api/artistApi";

export const artistKeys = {
  all: () => ["artist"] as const,
  byId: (id: number) => ["artist", id] as const,
};

export function useArtistQuery(id: number) {
  return useQuery({
    queryKey: artistKeys.byId(id),
    queryFn: () => artistApi.getArtist(id),
  });
}

export function useArtistByIdQuery(id: number) {
  return useQuery({
    queryKey: artistKeys.byId(id),
    queryFn: () => artistApi.getArtistById(id),
  });
}
