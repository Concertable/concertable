import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import {
  CONSENT_STORAGE_KEY,
  CONSENT_VERSION,
  DENIED_DECISION,
  hasConsent,
  isDecided,
  onConsentChange,
  readConsent,
  writeConsent,
  type ConsentRecord,
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
  it("returns null before any decision is stored", () => {
    expect(readConsent()).toBeNull();
    expect(isDecided(readConsent())).toBe(false);
  });

  it("stamps version + timestamp and round-trips a stored decision", () => {
    const written = writeConsent({ analytics: true, marketing: false });

    expect(written.version).toBe(CONSENT_VERSION);
    expect(Number.isNaN(Date.parse(written.decidedAtUtc))).toBe(false);
    expect(written.categories).toEqual({ analytics: true, marketing: false });
    expect(readConsent()).toEqual(written);
  });

  it("normalises unknown fields in the persisted decision", () => {
    const written = writeConsent({
      analytics: true,
      marketing: true,
      // a stray key must never survive into the stored record
      tracking: true,
    } as never);

    expect(written.categories).toEqual({ analytics: true, marketing: true });
  });

  it("invalidates a record written under a superseded version", () => {
    const stale: ConsentRecord = {
      version: CONSENT_VERSION - 1,
      decidedAtUtc: new Date().toISOString(),
      categories: { analytics: true, marketing: true },
    };
    localStorage.setItem(CONSENT_STORAGE_KEY, JSON.stringify(stale));

    expect(readConsent()).toBeNull();
  });

  it("treats absent, corrupt, or malformed storage as no decision", () => {
    localStorage.setItem(CONSENT_STORAGE_KEY, "not json");
    expect(readConsent()).toBeNull();

    localStorage.setItem(CONSENT_STORAGE_KEY, JSON.stringify({ version: 1 }));
    expect(readConsent()).toBeNull();
  });

  it("keeps every category off in the denied decision", () => {
    writeConsent(DENIED_DECISION);

    expect(hasConsent("analytics")).toBe(false);
    expect(hasConsent("marketing")).toBe(false);
  });

  it("reads the current stored record when no record is supplied", () => {
    writeConsent({ analytics: true, marketing: false });

    expect(hasConsent("analytics")).toBe(true);
    expect(hasConsent("marketing")).toBe(false);
  });

  it("honours an explicitly supplied record over storage", () => {
    writeConsent(DENIED_DECISION);
    const granted: ConsentRecord = {
      version: CONSENT_VERSION,
      decidedAtUtc: new Date().toISOString(),
      categories: { analytics: true, marketing: true },
    };

    expect(hasConsent("analytics", granted)).toBe(true);
  });

  it("notifies subscribers on write until they unsubscribe", () => {
    const seen: ConsentRecord[] = [];
    const unsubscribe = onConsentChange((record) => seen.push(record));

    writeConsent({ analytics: true, marketing: false });
    unsubscribe();
    writeConsent({ analytics: false, marketing: false });

    expect(seen).toHaveLength(1);
    expect(seen[0].categories).toEqual({ analytics: true, marketing: false });
  });
});
