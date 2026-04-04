import { Star } from "lucide-react";
import type { ReviewEntityType } from "@/api/reviewApi";
import { useReviews } from "@/hooks/useReviews";
import { PaginationControls } from "@/components/ui/PaginationControls";

interface Props {
  type: ReviewEntityType;
  id: number;
}

export function ReviewSection({ type, id }: Readonly<Props>) {
  const { reviews, summary, isLoading, params, nextPage, prevPage } =
    useReviews(type, id);

  return (
    <section className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold">Reviews</h2>
        {summary && (
          <div className="text-muted-foreground flex items-center gap-1 text-sm">
            <Star className="fill-gold text-gold size-4" />
            {summary.averageRating != null
              ? `${summary.averageRating.toFixed(1)} · ${summary.totalReviews} review${summary.totalReviews !== 1 ? "s" : ""}`
              : "No reviews yet"}
          </div>
        )}
      </div>

      {isLoading && (
        <p className="text-muted-foreground text-sm">Loading reviews...</p>
      )}

      {reviews && reviews.data.length === 0 && (
        <p className="text-muted-foreground text-sm">No reviews yet.</p>
      )}

      <ul className="space-y-4">
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
