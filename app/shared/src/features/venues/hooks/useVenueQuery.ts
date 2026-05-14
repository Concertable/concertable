import { useQuery } from "@tanstack/react-query";
import venueApi from "../api/venueApi";

export function useVenueQuery(id: number) {
  return useQuery({
    queryKey: ["venue", id],
    queryFn: () => venueApi.getVenue(id),
  });
}

export function useMyVenueQuery() {
  return useQuery({
    queryKey: ["venue", "my"],
    queryFn: venueApi.getMyVenue,
  });
}
