import { useReviewsQuery, useReviewSummaryQuery, useCanReviewQuery } from "@/hooks/query/useReviewQuery";
import { usePagination } from "@/hooks/usePagination";
import type { ReviewEntityType } from "@/api/reviewApi";

export function useReviews(type: ReviewEntityType, id: number) {
  const { params, setPage, nextPage, prevPage } = usePagination();
  const reviewsQuery = useReviewsQuery(type, id, params);
  const summaryQuery = useReviewSummaryQuery(type, id);
  const canReviewQuery = useCanReviewQuery(type, id);

  return {
    reviews: reviewsQuery.data,
    summary: summaryQuery.data,
    canReview: canReviewQuery.data ?? false,
    isLoading: reviewsQuery.isLoading,
    isError: reviewsQuery.isError,
    params,
    setPage,
    nextPage,
    prevPage,
  };
}
