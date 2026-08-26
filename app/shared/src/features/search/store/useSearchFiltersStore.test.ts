import { beforeEach, describe, expect, it } from "vitest";
import { useSearchFiltersStore } from "./useSearchFiltersStore";

describe("search filters store", () => {
  beforeEach(() => {
    useSearchFiltersStore.getState().replaceFilters({ headerType: "concert" });
  });

  it("updates selected filters without replacing the rest", () => {
    useSearchFiltersStore.getState().replaceFilters({
      headerType: "artist",
      query: "Echo",
    });

    useSearchFiltersStore.getState().updateFilters({ radius: 25 });

    expect(useSearchFiltersStore.getState().filters).toEqual({
      headerType: "artist",
      query: "Echo",
      radius: 25,
    });
  });

  it("replaces all filters", () => {
    useSearchFiltersStore.getState().updateFilters({ query: "Echo" });

    useSearchFiltersStore.getState().replaceFilters({ headerType: "venue" });

    expect(useSearchFiltersStore.getState().filters).toEqual({
      headerType: "venue",
    });
  });
});
