import { createContext, useContext, type ReactNode } from "react";
import type { ReviewEntityType } from "./types";

export type ReviewBasePath = (type: ReviewEntityType, id: number) => string;

export const b2bReviewBasePath: ReviewBasePath = (type, id) => {
  if (type === "concert") return `/concerts/${id}/reviews`;
  return `/${type}/${id}/review`;
};

export const customerReviewBasePath: ReviewBasePath = (type, id) =>
  `/${type}s/${id}/reviews`;

const ReviewRouteContext = createContext<ReviewBasePath>(customerReviewBasePath);

type Props = Readonly<{
  basePath: ReviewBasePath;
  children: ReactNode;
}>;

export function ReviewRouteProvider({ basePath, children }: Props) {
  return (
    <ReviewRouteContext.Provider value={basePath}>
      {children}
    </ReviewRouteContext.Provider>
  );
}

export function useReviewBasePath() {
  return useContext(ReviewRouteContext);
}
