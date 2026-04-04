import { useArtistQuery } from "@/hooks/query/useArtistQuery";
import type { Artist } from "@/types/artist";

export interface UseArtistResult {
  artist: Artist | undefined;
  isLoading: boolean;
  isError: boolean;
}

export function useArtist(id: number): UseArtistResult {
  const { data: artist, isLoading, isError } = useArtistQuery(id);
  return { artist, isLoading, isError };
}
