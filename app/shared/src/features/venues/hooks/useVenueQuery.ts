import { useQuery } from "@tanstack/react-query";
import venueApi from "../api/venueApi";

export const venueKeys = {
  all: () => ["venue"] as const,
  details: () => ["venue", "details"] as const,
  byId: (id: number) => ["venue", id] as const,
};

export function useVenueQuery() {
  return useQuery({
    queryKey: venueKeys.details(),
    queryFn: venueApi.getVenue,
  });
}

export function useVenueByIdQuery(id: number) {
  return useQuery({
    queryKey: venueKeys.byId(id),
    queryFn: () => venueApi.getVenueById(id),
  });
}
