import { apiClient } from "@concertable/shared/lib/apiClient";
import type { Pagination } from "@concertable/shared/types/common";
import type { PaginationParams } from "@concertable/shared/hooks/usePagination";
import type { Review, ReviewSummary, ReviewEntityType } from "../types";
import type { ReviewBasePath } from "../ReviewRouteProvider";

const reviewApi = {
  getReviews: async (
    basePath: ReviewBasePath,
    type: ReviewEntityType,
    id: number,
    params: PaginationParams,
  ): Promise<Pagination<Review>> => {
    const { data } = await apiClient.get<Pagination<Review>>(basePath(type, id), { params });
    return data;
  },

  getReviewSummary: async (
    basePath: ReviewBasePath,
    type: ReviewEntityType,
    id: number,
  ): Promise<ReviewSummary> => {
    const { data } = await apiClient.get<ReviewSummary>(`${basePath(type, id)}/summary`);
    return data;
  },
};

export default reviewApi;
