import { getRouteApi } from "@tanstack/react-router";
import { useVenue, VenueDetails } from "@/features/venues";
import { DetailsPageSkeleton } from "@/components/skeletons/DetailsPageSkeleton";

const routeApi = getRouteApi("/_customer/find/venue/$id");

export default function VenueDetailsPage() {
  const { id } = routeApi.useParams();
  const { venue, isLoading, isError } = useVenue(Number(id));

  if (isLoading) return <DetailsPageSkeleton sections={5} />;
  if (isError || !venue)
    return <div className="text-destructive p-6">Venue not found.</div>;

  return <VenueDetails venue={venue} />;
}
