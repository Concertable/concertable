export type ConsentCategory = "analytics" | "marketing";

export type ConsentDecision = Record<ConsentCategory, boolean>;

export interface StoredConsent {
  version: number;
  decidedAtUtc: string;
  categories: ConsentDecision;
}

export const CONSENT_STORAGE_KEY = "cookie-consent";

export const CONSENT_VERSION = 1;

export const CONSENT_CATEGORIES: readonly ConsentCategory[] = [
  "analytics",
  "marketing",
];

export const DENIED_DECISION: ConsentDecision = {
  analytics: false,
  marketing: false,
};

type ConsentListener = (record: StoredConsent) => void;

const listeners = new Set<ConsentListener>();

function subscribe(listener: ConsentListener): () => void {
  listeners.add(listener);
  return () => {
    listeners.delete(listener);
  };
}

function isStoredConsent(value: unknown): value is StoredConsent {
  if (typeof value !== "object" || value === null) return false;
  const record = value as Record<string, unknown>;
  if (typeof record.version !== "number") return false;
  if (typeof record.decidedAtUtc !== "string") return false;
  if (typeof record.categories !== "object" || record.categories === null)
    return false;
  const decision = record.categories as Record<string, unknown>;
  return CONSENT_CATEGORIES.every(
    (category) => typeof decision[category] === "boolean",
  );
}

function read(): StoredConsent | undefined {
  const raw = localStorage.getItem(CONSENT_STORAGE_KEY);
  if (raw === null) return undefined;
  let parsed: unknown;
  try {
    parsed = JSON.parse(raw);
  } catch {
    return undefined;
  }
  if (!isStoredConsent(parsed) || parsed.version !== CONSENT_VERSION)
    return undefined;
  return parsed;
}

function write(decision: ConsentDecision): StoredConsent {
  const categories = CONSENT_CATEGORIES.reduce(
    (result, category) => ({
      ...result,
      [category]: decision[category] === true,
    }),
    {} as ConsentDecision,
  );
  const record: StoredConsent = {
    version: CONSENT_VERSION,
    decidedAtUtc: new Date().toISOString(),
    categories,
  };
  localStorage.setItem(CONSENT_STORAGE_KEY, JSON.stringify(record));
  for (const listener of listeners) listener(record);
  return record;
}

function has(
  category: ConsentCategory,
  record: StoredConsent | undefined = read(),
): boolean {
  return record?.categories[category] === true;
}

export const consent = { has, read, subscribe, write };
