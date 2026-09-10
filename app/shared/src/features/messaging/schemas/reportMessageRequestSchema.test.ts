import { describe, expect, it } from "vitest";
import { reportMessageRequestSchema } from "./reportMessageRequestSchema";

describe("reportMessageRequestSchema", () => {
  it("normalizes optional details at the validation boundary", () => {
    expect(
      reportMessageRequestSchema.parse({
        category: "spam",
        details: "  Repeated links  ",
      }),
    ).toEqual({ category: "spam", details: "Repeated links" });
    expect(
      reportMessageRequestSchema.parse({ category: "spam", details: "  " }),
    ).toEqual({ category: "spam", details: undefined });
  });
});
