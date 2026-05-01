import { useQuery, keepPreviousData } from "@tanstack/react-query";
import { useAuth } from "react-oidc-context";
import reviewApi, { type ReviewEntityType } from "@/api/reviewApi";
import type { PaginationParams } from "@/hooks/usePagination";

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
  const { isAuthenticated } = useAuth();
  return useQuery({
    queryKey: ["reviews", type, id, "can-review"],
    queryFn: () => reviewApi.canReview(type, id),
    enabled: isAuthenticated,
  });
}
