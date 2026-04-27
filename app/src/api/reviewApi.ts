import api from "@/lib/axios";
import type { Review, ReviewSummary } from "@/types/review";
import type { Pagination } from "@/types/common";
import type { PaginationParams } from "@/hooks/usePagination";

export type ReviewEntityType = "venue" | "artist" | "concert";

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
    const { data } = await api.get<Pagination<Review>>(basePath(type, id), {
      params,
    });
    return data;
  },

  getReviewSummary: async (
    type: ReviewEntityType,
    id: number,
  ): Promise<ReviewSummary> => {
    const { data } = await api.get<ReviewSummary>(
      `${basePath(type, id)}/summary`,
    );
    return data;
  },

  canReview: async (type: ReviewEntityType, id: number): Promise<boolean> => {
    const { data } = await api.get<boolean>(
      `${basePath(type, id)}/eligibility`,
    );
    return data;
  },

  createReview: async (request: CreateReviewRequest): Promise<Review> => {
    const { concertId, ...body } = request;
    const { data } = await api.post<Review>(
      `${basePath("concert", concertId)}`,
      body,
    );
    return data;
  },
};

export default reviewApi;
