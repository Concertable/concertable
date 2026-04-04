import { getRouteApi } from "@tanstack/react-router";
import { useConcert } from "@/hooks/useConcert";
import { ConcertDetails } from "@/components/concert/ConcertDetails";

const routeApi = getRouteApi("/_customer/find/concert/$id");

export default function ConcertDetailsPage() {
  const { id } = routeApi.useParams();
  const { concert, isLoading, isError } = useConcert(Number(id));

  if (isLoading)
    return <div className="text-muted-foreground p-6">Loading...</div>;
  if (isError || !concert)
    return <div className="text-destructive p-6">Concert not found.</div>;

  return <ConcertDetails concert={concert} />;
}
