import { useQuery, keepPreviousData } from "@tanstack/react-query";
import type { PaginationParams } from "@/hooks/usePagination";
import reviewApi from "../api/reviewApi";
import type { ReviewEntityType } from "../types";

export function useReviewsQuery(
  type: ReviewEntityType,
  id: number,
  params: PaginationParams,
) {
  return useQuery({
    queryKey: ["reviews", type, id, params],
    queryFn: () => reviewApi.getReviews(type, id, params),
    placeholderData: keepPreviousData,
    enabled: !!id,
  });
}

export function useReviewSummaryQuery(type: ReviewEntityType, id: number) {
  return useQuery({
    queryKey: ["reviews", type, id, "summary"],
    queryFn: () => reviewApi.getReviewSummary(type, id),
    enabled: !!id,
  });
}
