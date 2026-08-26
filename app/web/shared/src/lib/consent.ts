export type ConsentCategory = "analytics" | "marketing";

export type ConsentDecision = Record<ConsentCategory, boolean>;

export interface ConsentRecord {
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

type ConsentListener = (record: ConsentRecord) => void;

const listeners = new Set<ConsentListener>();

export function onConsentChange(listener: ConsentListener): () => void {
  listeners.add(listener);
  return () => {
    listeners.delete(listener);
  };
}

function isConsentRecord(value: unknown): value is ConsentRecord {
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

export function readConsent(): ConsentRecord | null {
  const raw = localStorage.getItem(CONSENT_STORAGE_KEY);
  if (raw === null) return null;
  let parsed: unknown;
  try {
    parsed = JSON.parse(raw);
  } catch {
    return null;
  }
  if (!isConsentRecord(parsed) || parsed.version !== CONSENT_VERSION) return null;
  return parsed;
}

export function writeConsent(decision: ConsentDecision): ConsentRecord {
  const categories = CONSENT_CATEGORIES.reduce(
    (result, category) => ({ ...result, [category]: decision[category] === true }),
    {} as ConsentDecision,
  );
  const record: ConsentRecord = {
    version: CONSENT_VERSION,
    decidedAtUtc: new Date().toISOString(),
    categories,
  };
  localStorage.setItem(CONSENT_STORAGE_KEY, JSON.stringify(record));
  for (const listener of listeners) listener(record);
  return record;
}

export function hasConsent(
  category: ConsentCategory,
  record: ConsentRecord | null = readConsent(),
): boolean {
  return record?.categories[category] === true;
}

export function isDecided(record: ConsentRecord | null): boolean {
  return record !== null;
}
