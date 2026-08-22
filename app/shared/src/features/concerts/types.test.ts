import { describe, expect, it } from "vitest";
import { Concert, type Concert as ConcertRead } from "./types";

describe("Concert", () => {
  it("projects only writable fields into an update request", () => {
    const concert = {
      id: 42,
      name: "Before",
      about: "About",
      price: 20,
      totalTickets: 100,
      availableTickets: 80,
      startDate: "2026-09-01T19:00:00Z",
      endDate: "2026-09-01T22:00:00Z",
      rating: 4.8,
      venue: { id: 1 },
      artist: { id: 2 },
      genres: ["rock"],
    } as ConcertRead;

    expect(Concert.toUpdateRequest(concert)).toEqual({
      name: "Before",
      about: "About",
      price: 20,
      totalTickets: 100,
    });
  });
});
