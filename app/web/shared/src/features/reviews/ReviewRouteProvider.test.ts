import { describe, expect, it } from "vitest";
import { b2bReviewBasePath, customerReviewBasePath } from "./ReviewRouteProvider";

describe("review route base paths", () => {
  it.each([
    ["artist", 12, "/artist/12/review"],
    ["venue", 34, "/venue/34/review"],
    ["concert", 56, "/concerts/56/reviews"],
  ] as const)("maps B2B %s routes", (type, id, expected) => {
    expect(b2bReviewBasePath(type, id)).toBe(expected);
  });

  it.each([
    ["artist", 12, "/artists/12/reviews"],
    ["venue", 34, "/venues/34/reviews"],
    ["concert", 56, "/concerts/56/reviews"],
  ] as const)("maps customer %s routes", (type, id, expected) => {
    expect(customerReviewBasePath(type, id)).toBe(expected);
  });
});
