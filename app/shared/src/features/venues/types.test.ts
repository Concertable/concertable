import { describe, expect, it } from "vitest";
import { Venue, type Venue as VenueRead } from "./types";

describe("Venue", () => {
  it("projects only writable fields into an update request", () => {
    const venue: VenueRead = {
      id: 42,
      name: "Example Venue",
      about: "About",
      bannerUrl: "banner",
      avatar: "avatar",
      rating: 4.5,
      email: "venue@example.com",
      county: "Greater London",
      town: "London",
      latitude: 51.5,
      longitude: -0.1,
    };

    expect(Venue.toUpdateRequest(venue)).toEqual({
      name: "Example Venue",
      about: "About",
      latitude: 51.5,
      longitude: -0.1,
    });
  });
});
