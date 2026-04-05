import { Star } from "lucide-react";
import type { ReviewEntityType } from "@/api/reviewApi";
import { useReviewSummaryQuery } from "@/hooks/query/useReviewQuery";

interface Props {
  type: ReviewEntityType;
  id: number;
}

export function ReviewSummaryBadge({ type, id }: Readonly<Props>) {
  const { data: summary } = useReviewSummaryQuery(type, id);
  if (!summary) return null;

  return (
    <div className="text-muted-foreground flex items-center gap-1 text-sm">
      <Star className="fill-gold text-gold size-4" />
      {summary.averageRating != null
        ? `${summary.averageRating.toFixed(1)} · ${summary.totalReviews} review${summary.totalReviews !== 1 ? "s" : ""}`
        : "No reviews yet"}
    </div>
  );
}
