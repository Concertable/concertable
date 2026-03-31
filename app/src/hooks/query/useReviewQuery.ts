import { useQuery, keepPreviousData } from "@tanstack/react-query";
import { getReviews, getReviewSummary, canReview } from "@/api/reviewApi";
import type { ReviewEntityType } from "@/api/reviewApi";
import type { PaginationParams } from "@/hooks/usePagination";

export function useReviewsQuery(type: ReviewEntityType, id: number, params: PaginationParams) {
  return useQuery({
    queryKey: ["reviews", type, id, params],
    queryFn: () => getReviews(type, id, params),
    placeholderData: keepPreviousData,
  });
}

export function useReviewSummaryQuery(type: ReviewEntityType, id: number) {
  return useQuery({
    queryKey: ["reviews", type, id, "summary"],
    queryFn: () => getReviewSummary(type, id),
  });
}

export function useCanReviewQuery(type: ReviewEntityType, id: number) {
  return useQuery({
    queryKey: ["reviews", type, id, "can-review"],
    queryFn: () => canReview(type, id),
  });
}
