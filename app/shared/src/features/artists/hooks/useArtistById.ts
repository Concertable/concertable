import { useArtistByIdQuery } from "./useArtistQuery";
import type { Artist } from "../types";

export interface UseArtistByIdResult {
  artist: Artist | undefined;
  isLoading: boolean;
  isError: boolean;
}

export function useArtistById(id: number): UseArtistByIdResult {
  const { data: artist, isLoading, isError } = useArtistByIdQuery(id);
  return { artist, isLoading, isError };
}
