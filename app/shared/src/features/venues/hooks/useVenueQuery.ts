import { useQuery } from "@tanstack/react-query";
import venueApi from "../api/venueApi";

export const venueKeys = {
  all: () => ["venue"] as const,
  byId: (id: number) => ["venue", id] as const,
  my: () => ["venue", "my"] as const,
};

export function useVenueQuery(id: number) {
  return useQuery({
    queryKey: venueKeys.byId(id),
    queryFn: () => venueApi.getVenue(id),
  });
}

export function useMyVenueQuery() {
  return useQuery({
    queryKey: venueKeys.my(),
    queryFn: venueApi.getMyVenue,
  });
}
