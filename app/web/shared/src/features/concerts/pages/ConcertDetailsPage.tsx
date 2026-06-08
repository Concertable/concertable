import type { ReactNode } from "react";
import { DetailsPageSkeleton } from "@/components/skeletons/DetailsPageSkeleton";
import { ConcertDetails } from "../components/ConcertDetails";
import { useConcert } from "../hooks/useConcert";

interface Props {
  id: number;
  addReviewSlot?: ReactNode;
  onBuyTickets?: () => void;
}

export function ConcertDetailsPage({ id, addReviewSlot, onBuyTickets }: Readonly<Props>) {
  const { concert, isLoading, isError } = useConcert(id);

  if (isLoading) return <DetailsPageSkeleton sections={4} />;
  if (isError || !concert)
    return <div className="text-destructive p-6">Concert not found.</div>;

  return (
    <ConcertDetails
      concert={concert}
      addReviewSlot={addReviewSlot}
      onBuyTickets={onBuyTickets}
    />
  );
}
