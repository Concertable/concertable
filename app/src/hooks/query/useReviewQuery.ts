import { useQuery, keepPreviousData } from "@tanstack/react-query";
import * as reviewApi from "@/api/reviewApi";
import type { ReviewEntityType } from "@/api/reviewApi";
import type { PaginationParams } from "@/hooks/usePagination";
import { useAuthStore } from "@/store/useAuthStore";

export function useReviewsQuery(
  type: ReviewEntityType,
  id: number,
  params: PaginationParams,
) {
  return useQuery({
    queryKey: ["reviews", type, id, params],
    queryFn: () => reviewApi.getReviews(type, id, params),
    placeholderData: keepPreviousData,
  });
}

export function useReviewSummaryQuery(type: ReviewEntityType, id: number) {
  return useQuery({
    queryKey: ["reviews", type, id, "summary"],
    queryFn: () => reviewApi.getReviewSummary(type, id),
  });
}

export function useCanReviewQuery(type: ReviewEntityType, id: number) {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  return useQuery({
    queryKey: ["reviews", type, id, "can-review"],
    queryFn: () => reviewApi.canReview(type, id),
    enabled: isAuthenticated,
  });
}
