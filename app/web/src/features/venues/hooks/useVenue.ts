import { useVenueQuery } from "./useVenueQuery";
import type { Venue } from "../types";

export interface UseVenueResult {
  venue: Venue | undefined;
  isLoading: boolean;
  isError: boolean;
}

export function useVenue(id: number): UseVenueResult {
  const { data: venue, isLoading, isError } = useVenueQuery(id);
  return { venue, isLoading, isError };
}
