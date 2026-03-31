export interface Review {
  id: number;
  email: string;
  stars: number;
  details?: string;
}

export interface ReviewSummary {
  totalReviews: number;
  averageRating?: number;
}
