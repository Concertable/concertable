import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { registerConsentGated, registerConsentGatedScript } from "./consentGate";
import { DENIED_DECISION, writeConsent } from "./consent";

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

interface FakeScript {
  src: string;
  async: boolean;
  id: string;
  attrs: Record<string, string>;
  removed: boolean;
}

function createFakeDocument(appended: FakeScript[]): Document {
  const doc = {
    head: {
      appendChild: (element: FakeScript) => {
        appended.push(element);
        return element;
      },
    },
    createElement: () => {
      const element: FakeScript = {
        src: "",
        async: false,
        id: "",
        attrs: {},
        removed: false,
        setAttribute(name: string, value: string) {
          this.attrs[name] = value;
        },
        remove() {
          this.removed = true;
          const index = appended.indexOf(this);
          if (index >= 0) appended.splice(index, 1);
        },
      } as unknown as FakeScript;
      return element;
    },
  };
  return doc as unknown as Document;
}

beforeEach(() => {
  vi.stubGlobal("localStorage", createMemoryStorage());
});

afterEach(() => {
  vi.unstubAllGlobals();
});

describe("registerConsentGated", () => {
  it("does not activate while the category is undecided", () => {
    const activate = vi.fn();
    const teardown = registerConsentGated({ category: "analytics", activate });

    expect(activate).not.toHaveBeenCalled();
    teardown();
  });

  it("activates immediately when consent is already granted at registration", () => {
    writeConsent({ analytics: true, marketing: false });
    const activate = vi.fn();
    const teardown = registerConsentGated({ category: "analytics", activate });

    expect(activate).toHaveBeenCalledTimes(1);
    teardown();
  });

  it("activates on the grant edge and deactivates on the withdraw edge", () => {
    const activate = vi.fn();
    const deactivate = vi.fn();
    const teardown = registerConsentGated({
      category: "marketing",
      activate,
      deactivate,
    });

    writeConsent({ analytics: false, marketing: true });
    expect(activate).toHaveBeenCalledTimes(1);
    expect(deactivate).not.toHaveBeenCalled();

    writeConsent(DENIED_DECISION);
    expect(deactivate).toHaveBeenCalledTimes(1);
    teardown();
  });

  it("fires once per edge, not on unrelated re-grants", () => {
    const activate = vi.fn();
    const teardown = registerConsentGated({ category: "analytics", activate });

    writeConsent({ analytics: true, marketing: false });
    writeConsent({ analytics: true, marketing: true });

    expect(activate).toHaveBeenCalledTimes(1);
    teardown();
  });

  it("ignores changes to other categories", () => {
    const activate = vi.fn();
    const teardown = registerConsentGated({ category: "analytics", activate });

    writeConsent({ analytics: false, marketing: true });

    expect(activate).not.toHaveBeenCalled();
    teardown();
  });

  it("stops reacting after teardown", () => {
    const activate = vi.fn();
    const teardown = registerConsentGated({ category: "analytics", activate });
    teardown();

    writeConsent({ analytics: true, marketing: false });

    expect(activate).not.toHaveBeenCalled();
  });
});

describe("registerConsentGatedScript", () => {
  it("injects the script on grant and removes it on withdrawal", () => {
    const appended: FakeScript[] = [];
    vi.stubGlobal("document", createFakeDocument(appended));

    const teardown = registerConsentGatedScript({
      category: "analytics",
      src: "https://example.test/tag.js",
      id: "ga-tag",
      attributes: { "data-domain": "concertable" },
    });

    writeConsent({ analytics: true, marketing: false });
    expect(appended).toHaveLength(1);
    expect(appended[0].src).toBe("https://example.test/tag.js");
    expect(appended[0].async).toBe(true);
    expect(appended[0].id).toBe("ga-tag");
    expect(appended[0].attrs["data-domain"]).toBe("concertable");

    writeConsent(DENIED_DECISION);
    expect(appended).toHaveLength(0);
    teardown();
  });
});
