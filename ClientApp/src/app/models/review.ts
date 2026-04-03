export interface Review {
    id: number;
    email: string;
    stars: number;
    details?: string;
}

export interface CreateReview {
    eventId: number;
    stars: number;
    details?: string;
}