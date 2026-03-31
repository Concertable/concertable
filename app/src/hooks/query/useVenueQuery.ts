import { useQuery } from "@tanstack/react-query";
import { getVenue, getMyVenue } from "@/api/venueApi";

export function useVenueQuery(id: number) {
  return useQuery({
    queryKey: ["venue", id],
    queryFn: () => getVenue(id),
  });
}

export function useMyVenueQuery() {
  return useQuery({
    queryKey: ["venue", "my"],
    queryFn: getMyVenue,
  });
}
