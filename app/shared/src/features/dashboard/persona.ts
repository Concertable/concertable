import dayjs from "dayjs";

export type FixturePersona = "empty" | "mid" | "thriving";

const VALID: FixturePersona[] = ["empty", "mid", "thriving"];

export function selectPersona(defaultPersona: FixturePersona = "mid"): FixturePersona {
  if (typeof window === "undefined") return defaultPersona;
  const raw = new URLSearchParams(window.location.search).get("persona");
  if (raw && (VALID as string[]).includes(raw)) return raw as FixturePersona;
  return defaultPersona;
}

export const NOW = dayjs("2026-05-18T12:00:00Z");

export function daysAhead(days: number): string {
  return NOW.add(days, "day").toISOString();
}

export function daysAgo(days: number): string {
  return NOW.subtract(days, "day").toISOString();
}

export function hoursAgo(hours: number): string {
  return NOW.subtract(hours, "hour").toISOString();
}

export function monthsAgoIso(months: number): string {
  return NOW.subtract(months, "month").startOf("month").format("YYYY-MM-DD");
}
