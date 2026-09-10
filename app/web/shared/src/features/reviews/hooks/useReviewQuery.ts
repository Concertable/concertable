import { useQuery, keepPreviousData } from "@tanstack/react-query";
import type { PaginationParams } from "@/hooks/usePagination";
import reviewApi from "../api/reviewApi";
import { useReviewBasePath } from "../ReviewRouteProvider";
import type { ReviewEntityType } from "../types";

export function useReviewsQuery(
  type: ReviewEntityType,
  id: number,
  params: PaginationParams,
) {
  const basePath = useReviewBasePath();
  return useQuery({
    queryKey: ["reviews", type, id, params],
    queryFn: () => reviewApi.getReviews(basePath, type, id, params),
    placeholderData: keepPreviousData,
    enabled: !!id,
  });
}

export function useReviewSummaryQuery(type: ReviewEntityType, id: number) {
  const basePath = useReviewBasePath();
  return useQuery({
    queryKey: ["reviews", type, id, "summary"],
    queryFn: () => reviewApi.getReviewSummary(basePath, type, id),
    enabled: !!id,
  });
}
