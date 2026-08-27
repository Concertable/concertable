import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import {
  CONSENT_STORAGE_KEY,
  CONSENT_VERSION,
  DENIED_DECISION,
  consent,
  type StoredConsent,
} from "./consent";

function createMemoryStorage(): Storage {
  const store = new Map<string, string>();
  return {
    getItem: (key) => (store.has(key) ? store.get(key)! : null),
    setItem: (key, value) => void store.set(key, String(value)),
    removeItem: (key) => void store.delete(key),
    clear: () => store.clear(),
    key: (index) => [...store.keys()][index] ?? null,
    get length() {
      return store.size;
    },
  } as Storage;
}

beforeEach(() => {
  vi.stubGlobal("localStorage", createMemoryStorage());
});

afterEach(() => {
  vi.unstubAllGlobals();
});

describe("consent record", () => {
  it("returns undefined before any decision is stored", () => {
    expect(consent.read()).toBeUndefined();
  });

  it("stamps version + timestamp and round-trips a stored decision", () => {
    const written = consent.write({ analytics: true, marketing: false });

    expect(written.version).toBe(CONSENT_VERSION);
    expect(Number.isNaN(Date.parse(written.decidedAtUtc))).toBe(false);
    expect(written.categories).toEqual({ analytics: true, marketing: false });
    expect(consent.read()).toEqual(written);
  });

  it("normalises unknown fields in the persisted decision", () => {
    const written = consent.write({
      analytics: true,
      marketing: true,
      tracking: true,
    } as never);

    expect(written.categories).toEqual({ analytics: true, marketing: true });
  });

  it("invalidates a record written under a superseded version", () => {
    const stale: StoredConsent = {
      version: CONSENT_VERSION - 1,
      decidedAtUtc: new Date().toISOString(),
      categories: { analytics: true, marketing: true },
    };
    localStorage.setItem(CONSENT_STORAGE_KEY, JSON.stringify(stale));

    expect(consent.read()).toBeUndefined();
  });

  it("treats absent, corrupt, or malformed storage as no decision", () => {
    localStorage.setItem(CONSENT_STORAGE_KEY, "not json");
    expect(consent.read()).toBeUndefined();

    localStorage.setItem(CONSENT_STORAGE_KEY, JSON.stringify({ version: 1 }));
    expect(consent.read()).toBeUndefined();
  });

  it("keeps every category off in the denied decision", () => {
    consent.write(DENIED_DECISION);

    expect(consent.has("analytics")).toBe(false);
    expect(consent.has("marketing")).toBe(false);
  });

  it("reads the current stored record when no record is supplied", () => {
    consent.write({ analytics: true, marketing: false });

    expect(consent.has("analytics")).toBe(true);
    expect(consent.has("marketing")).toBe(false);
  });

  it("honours an explicitly supplied record over storage", () => {
    consent.write(DENIED_DECISION);
    const granted: StoredConsent = {
      version: CONSENT_VERSION,
      decidedAtUtc: new Date().toISOString(),
      categories: { analytics: true, marketing: true },
    };

    expect(consent.has("analytics", granted)).toBe(true);
  });

  it("notifies subscribers on write until they unsubscribe", () => {
    const seen: StoredConsent[] = [];
    const unsubscribe = consent.subscribe((record) => seen.push(record));

    consent.write({ analytics: true, marketing: false });
    unsubscribe();
    consent.write({ analytics: false, marketing: false });

    expect(seen).toHaveLength(1);
    expect(seen[0].categories).toEqual({ analytics: true, marketing: false });
  });
});
