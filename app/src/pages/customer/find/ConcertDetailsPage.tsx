import { getRouteApi } from "@tanstack/react-router";
import { useConcert } from "@/hooks/useConcert";
import { ConcertDetails } from "@/components/concert/ConcertDetails";

const routeApi = getRouteApi("/_customer/find/concert/$id");

export default function ConcertDetailsPage() {
  const { id } = routeApi.useParams();
  const { concert, isLoading, isError } = useConcert(Number(id));

  if (isLoading) return <div className="p-6 text-muted-foreground">Loading...</div>;
  if (isError || !concert) return <div className="p-6 text-destructive">Concert not found.</div>;

  return <ConcertDetails concert={concert} />;
}
