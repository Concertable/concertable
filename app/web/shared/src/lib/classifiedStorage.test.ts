import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

vi.mock("@/lib/consent", () => ({
  consent: { has: vi.fn() },
}));

vi.mock("@/lib/storageManifest", () => ({
  STORAGE_MANIFEST: [
    {
      key: "func-local",
      api: "localStorage",
      firstParty: true,
      classification: "functional",
      owner: "first-party",
      purpose: "",
      duration: "",
      apps: [],
    },
    {
      key: "func-session",
      api: "sessionStorage",
      firstParty: true,
      classification: "functional",
      owner: "first-party",
      purpose: "",
      duration: "",
      apps: [],
    },
    {
      key: "analytics-item",
      api: "localStorage",
      firstParty: true,
      classification: "analytics",
      consentCategory: "analytics",
      owner: "first-party",
      purpose: "",
      duration: "",
      apps: [],
    },
    {
      key: "cookie-item",
      api: "cookie",
      firstParty: true,
      classification: "necessary",
      owner: "first-party",
      purpose: "",
      duration: "",
      apps: [],
    },
    {
      key: "third-party",
      api: "localStorage",
      firstParty: false,
      classification: "necessary",
      owner: "vendor",
      purpose: "",
      duration: "",
      apps: [],
    },
  ],
}));

import { consent } from "@/lib/consent";
import { createClassifiedStorage } from "./classifiedStorage";

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
  vi.stubGlobal("sessionStorage", createMemoryStorage());
  vi.mocked(consent.has).mockReset();
});

afterEach(() => {
  vi.unstubAllGlobals();
});

describe("createClassifiedStorage", () => {
  it("refuses a key with no first-party manifest entry", () => {
    expect(() => createClassifiedStorage("unknown")).toThrow(/classify it/);
    expect(() => createClassifiedStorage("third-party")).toThrow(/classify it/);
  });

  it("refuses an item that is not web storage", () => {
    expect(() => createClassifiedStorage("cookie-item")).toThrow(
      /web storage only/,
    );
  });

  it("reads, writes, and removes a functional item without consent", () => {
    const storage = createClassifiedStorage("func-local");

    expect(storage.get()).toBeNull();
    expect(storage.set("dark")).toBe(true);
    expect(storage.get()).toBe("dark");
    expect(localStorage.getItem("func-local")).toBe("dark");

    storage.remove();
    expect(storage.get()).toBeNull();
    expect(consent.has).not.toHaveBeenCalled();
  });

  it("targets the api named in the manifest entry", () => {
    createClassifiedStorage("func-session").set("value");

    expect(sessionStorage.getItem("func-session")).toBe("value");
    expect(localStorage.getItem("func-session")).toBeNull();
  });

  it("withholds a consent-gated write until its category is granted", () => {
    vi.mocked(consent.has).mockReturnValue(false);
    const storage = createClassifiedStorage("analytics-item");

    expect(storage.set("id")).toBe(false);
    expect(localStorage.getItem("analytics-item")).toBeNull();
    expect(consent.has).toHaveBeenCalledWith("analytics");
  });

  it("writes a consent-gated item once its category is granted", () => {
    vi.mocked(consent.has).mockReturnValue(true);
    const storage = createClassifiedStorage("analytics-item");

    expect(storage.set("id")).toBe(true);
    expect(localStorage.getItem("analytics-item")).toBe("id");
  });
});
