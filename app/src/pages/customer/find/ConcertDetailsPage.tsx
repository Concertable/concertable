import { getRouteApi } from "@tanstack/react-router";
import { useConcert } from "@/hooks/useConcert";
import { ConcertDetails } from "@/components/concert/ConcertDetails";
import { DetailsPageSkeleton } from "@/components/skeletons/DetailsPageSkeleton";

const routeApi = getRouteApi("/_customer/find/concert/$id");

export default function ConcertDetailsPage() {
  const { id } = routeApi.useParams();
  const { concert, isLoading, isError } = useConcert(Number(id));

  if (isLoading) return <DetailsPageSkeleton sections={4} />;
  if (isError || !concert)
    return <div className="text-destructive p-6">Concert not found.</div>;

  return <ConcertDetails concert={concert} />;
}
