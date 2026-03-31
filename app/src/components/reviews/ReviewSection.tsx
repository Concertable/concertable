import { Star } from "lucide-react";
import type { ReviewEntityType } from "@/api/reviewApi";
import { useReviews } from "@/hooks/useReviews";

interface Props {
  type: ReviewEntityType;
  id: number;
}

export function ReviewSection({ type, id }: Readonly<Props>) {
  const { reviews, summary, isLoading, params, nextPage, prevPage } = useReviews(type, id);

  const totalPages = reviews ? Math.ceil(reviews.totalCount / params.pageSize) : 0;

  return (
    <section className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold">Reviews</h2>
        {summary && (
          <div className="flex items-center gap-1 text-sm text-muted-foreground">
            <Star className="size-4 fill-yellow-400 text-yellow-400" />
            {summary.averageRating != null
              ? `${summary.averageRating.toFixed(1)} · ${summary.totalReviews} review${summary.totalReviews !== 1 ? "s" : ""}`
              : "No reviews yet"}
          </div>
        )}
      </div>

      {isLoading && <p className="text-muted-foreground text-sm">Loading reviews...</p>}

      {reviews && reviews.data.length === 0 && (
        <p className="text-muted-foreground text-sm">No reviews yet.</p>
      )}

      <ul className="space-y-4">
        {reviews?.data.map((review) => (
          <li key={review.id} className="border border-border rounded-lg p-4 space-y-1">
            <div className="flex items-center justify-between">
              <span className="text-sm font-medium">{review.email}</span>
              <div className="flex items-center gap-0.5">
                {Array.from({ length: 5 }).map((_, i) => (
                  <Star
                    key={i}
                    className={`size-3.5 ${i < review.stars ? "fill-yellow-400 text-yellow-400" : "text-muted-foreground"}`}
                  />
                ))}
              </div>
            </div>
            {review.details && <p className="text-sm text-muted-foreground">{review.details}</p>}
          </li>
        ))}
      </ul>

      {totalPages > 1 && (
        <div className="flex items-center justify-end gap-2 text-sm">
          <button
            onClick={prevPage}
            disabled={params.pageNumber === 1}
            className="px-3 py-1 rounded border border-border disabled:opacity-40"
          >
            Previous
          </button>
          <span className="text-muted-foreground">{params.pageNumber} / {totalPages}</span>
          <button
            onClick={nextPage}
            disabled={params.pageNumber === totalPages}
            className="px-3 py-1 rounded border border-border disabled:opacity-40"
          >
            Next
          </button>
        </div>
      )}
    </section>
  );
}
