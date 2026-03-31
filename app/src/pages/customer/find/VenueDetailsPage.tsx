import { getRouteApi } from "@tanstack/react-router";
import { useVenue } from "@/hooks/useVenue";
import { VenueDetails } from "@/components/venue/VenueDetails";

const routeApi = getRouteApi("/_customer/find/venue/$id");

export default function VenueDetailsPage() {
  const { id } = routeApi.useParams();
  const { venue, isLoading, isError } = useVenue(Number(id));

  if (isLoading) return <div className="p-6 text-muted-foreground">Loading...</div>;
  if (isError || !venue) return <div className="p-6 text-destructive">Venue not found.</div>;

  return <VenueDetails venue={venue} />;
}
