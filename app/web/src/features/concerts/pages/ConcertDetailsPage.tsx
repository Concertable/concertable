import { useConcert } from "../hooks/useConcert";
import { ConcertDetails } from "../components/ConcertDetails";
import { DetailsPageSkeleton } from "@/components/skeletons/DetailsPageSkeleton";

interface Props {
  id: number;
}

export function ConcertDetailsPage({ id }: Props) {
  const { concert, isLoading, isError } = useConcert(id);

  if (isLoading) return <DetailsPageSkeleton sections={4} />;
  if (isError || !concert)
    return <div className="text-destructive p-6">Concert not found.</div>;

  return <ConcertDetails concert={concert} />;
}
