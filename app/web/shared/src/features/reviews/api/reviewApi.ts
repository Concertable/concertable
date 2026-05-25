import api from "@concertable/shared/lib/axiosClient";
import customerApi from "@concertable/shared/lib/customerAxiosClient";
import type { Pagination } from "@concertable/shared/types/common";
import type { PaginationParams } from "@concertable/shared/hooks/usePagination";
import type { Review, ReviewSummary, ReviewEntityType } from "../types";

interface CreateReviewRequest {
  concertId: number;
  stars: number;
  details?: string;
}

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

  canReview: async (type: ReviewEntityType, id: number): Promise<boolean> => {
    const { data } = await customerApi.get<boolean>(`${basePath(type, id)}/eligibility`);
    return data;
  },

  createReview: async (request: CreateReviewRequest): Promise<Review> => {
    const { concertId, ...body } = request;
    const { data } = await customerApi.post<Review>(`${basePath("concert", concertId)}`, body);
    return data;
  },
};

export default reviewApi;
