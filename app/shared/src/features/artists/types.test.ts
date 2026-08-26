import { describe, expect, it } from "vitest";
import { Artist, type Artist as ArtistRead } from "./types";

describe("Artist", () => {
  it("projects only writable fields into an update request", () => {
    const artist: ArtistRead = {
      id: 42,
      name: "Example Artist",
      about: "About",
      bannerUrl: "banner",
      avatar: "avatar",
      rating: 4.5,
      genres: ["rock", "jazz"],
      email: "artist@example.com",
      county: "Greater London",
      town: "London",
      latitude: 51.5,
      longitude: -0.1,
    };

    expect(Artist.toUpdateRequest(artist)).toEqual({
      name: "Example Artist",
      about: "About",
      latitude: 51.5,
      longitude: -0.1,
      genres: ["rock", "jazz"],
    });
  });
});
