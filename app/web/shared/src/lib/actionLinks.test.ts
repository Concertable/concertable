import { describe, expect, it } from "vitest";
import { visibleActionNames } from "./actionLinks";

const labels = {
  accept: "Accept",
  checkout: "Continue",
  cancel: "Cancel",
  contract: "Contract",
};

const link = { href: "/api/x", method: "POST" };

describe("visibleActionNames", () => {
  it("keeps only the names the payload advertises", () => {
    expect(
      visibleActionNames({ cancel: link, contract: link }, labels),
    ).toEqual(["cancel", "contract"]);
  });

  it("orders by the label map rather than the payload key order", () => {
    expect(
      visibleActionNames({ contract: link, accept: link }, labels),
    ).toEqual(["accept", "contract"]);
  });

  it("applies the caller's filter on top of link presence", () => {
    expect(
      visibleActionNames(
        { accept: link, checkout: link },
        labels,
        (name) => name !== "accept",
      ),
    ).toEqual(["checkout"]);
  });

  it("returns nothing when the payload advertises no links", () => {
    expect(visibleActionNames({}, labels)).toEqual([]);
  });
});
