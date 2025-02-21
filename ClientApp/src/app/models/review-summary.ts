import { Pagination } from "./pagination";
import { Review } from "./review";

export interface ReviewSummary {
    messages: Pagination<Review>
    averageRating: number;
}