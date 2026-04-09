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

const reviewApi = {
  getReviews: async (
    type: ReviewEntityType,
    id: number,
    params: PaginationParams,
  ): Promise<Pagination<Review>> => {
    const { data } = await api.get<Pagination<Review>>(
      `/review/${type}/${id}`,
      { params },
    );
    return data;
  },

  getReviewSummary: async (
    type: ReviewEntityType,
    id: number,
  ): Promise<ReviewSummary> => {
    const { data } = await api.get<ReviewSummary>(
      `/review/${type}/summary/${id}`,
    );
    return data;
  },

  canReview: async (type: ReviewEntityType, id: number): Promise<boolean> => {
    const { data } = await api.get<boolean>(`/review/${type}/can-review/${id}`);
    return data;
  },

  createReview: async (request: CreateReviewRequest): Promise<Review> => {
    const { data } = await api.post<Review>("/review", request);
    return data;
  },
};

export default reviewApi;
