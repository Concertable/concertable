import { Star } from "lucide-react";
import { PaginationControls } from "@/components/ui/PaginationControls";
import { Skeleton } from "@/components/ui/skeleton";
import { useReviews } from "../hooks/useReviews";
import type { ReviewEntityType } from "../types";

interface Props {
  type: ReviewEntityType;
  id: number;
}

function ReviewSectionSkeleton() {
  return (
    <ul className="space-y-4">
      {Array.from({ length: 3 }).map((_, i) => (
        <li key={i} className="border-border space-y-2 rounded-lg border p-4">
          <div className="flex items-center justify-between">
            <Skeleton className="h-4 w-36" />
            <Skeleton className="h-3.5 w-20" />
          </div>
          <Skeleton className="h-4 w-3/4" />
        </li>
      ))}
    </ul>
  );
}

export function ReviewSection({ type, id }: Readonly<Props>) {
  const { reviews, isLoading, params, nextPage, prevPage } = useReviews(
    type,
    id,
  );

  return (
    <section className="space-y-4">
      {isLoading && <ReviewSectionSkeleton />}

      {reviews && reviews.data.length === 0 && (
        <p className="text-muted-foreground text-sm">No reviews yet.</p>
      )}

      <ul className="min-h-[420px] space-y-4">
        {reviews?.data.map((review) => (
          <li
            key={review.id}
            className="border-border space-y-1 rounded-lg border p-4"
          >
            <div className="flex items-center justify-between">
              <span className="text-sm font-medium">{review.email}</span>
              <div className="flex items-center gap-0.5">
                {Array.from({ length: 5 }).map((_, i) => (
                  <Star
                    key={i}
                    className={`size-3.5 ${i < review.stars ? "fill-gold text-gold" : "text-muted-foreground"}`}
                  />
                ))}
              </div>
            </div>
            {review.details && (
              <p className="text-muted-foreground text-sm">{review.details}</p>
            )}
          </li>
        ))}
      </ul>

      <PaginationControls
        pageNumber={params.pageNumber}
        totalPages={reviews?.totalPages ?? 0}
        onPrev={prevPage}
        onNext={nextPage}
      />
    </section>
  );
}
