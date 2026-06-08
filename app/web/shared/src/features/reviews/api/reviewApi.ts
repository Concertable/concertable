import api from "@concertable/shared/lib/axiosClient";
import type { Pagination } from "@concertable/shared/types/common";
import type { PaginationParams } from "@concertable/shared/hooks/usePagination";
import type { Review, ReviewSummary, ReviewEntityType } from "../types";

const basePath = (type: ReviewEntityType, id: number) =>
  `/${type}s/${id}/reviews`;

const reviewApi = {
  getReviews: async (
    type: ReviewEntityType,
    id: number,
    params: PaginationParams,
  ): Promise<Pagination<Review>> => {
    const { data } = await api.get<Pagination<Review>>(basePath(type, id), { params });
    return data;
  },

  getReviewSummary: async (type: ReviewEntityType, id: number): Promise<ReviewSummary> => {
    const { data } = await api.get<ReviewSummary>(`${basePath(type, id)}/summary`);
    return data;
  },
};

export default reviewApi;
