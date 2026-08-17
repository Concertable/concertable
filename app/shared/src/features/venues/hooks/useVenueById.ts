import { useVenueByIdQuery } from "./useVenueQuery";
import type { Venue } from "../types";

export interface UseVenueByIdResult {
  venue: Venue | undefined;
  isLoading: boolean;
  isError: boolean;
}

export function useVenueById(id: number): UseVenueByIdResult {
  const { data: venue, isLoading, isError } = useVenueByIdQuery(id);
  return { venue, isLoading, isError };
}
