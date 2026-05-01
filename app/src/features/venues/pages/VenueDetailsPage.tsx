import { useVenue } from "../hooks/useVenue";
import { VenueDetails } from "../components/VenueDetails";
import { DetailsPageSkeleton } from "@/components/skeletons/DetailsPageSkeleton";

interface Props {
  id: number;
}

export function VenueDetailsPage({ id }: Props) {
  const { venue, isLoading, isError } = useVenue(id);

  if (isLoading) return <DetailsPageSkeleton sections={5} />;
  if (isError || !venue)
    return <div className="text-destructive p-6">Venue not found.</div>;

  return <VenueDetails venue={venue} />;
}
