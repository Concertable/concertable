import { createContext, useContext, useState, type ReactNode } from "react";
import {
  CONSENT_CATEGORIES,
  DENIED_DECISION,
  hasConsent as hasConsentInRecord,
  readConsent,
  writeConsent,
  type ConsentCategory,
  type ConsentDecision,
  type ConsentRecord,
} from "@/lib/consent";

const GRANTED_DECISION: ConsentDecision = CONSENT_CATEGORIES.reduce(
  (decision, category) => ({ ...decision, [category]: true }),
  {} as ConsentDecision,
);

interface ConsentContextValue {
  record: ConsentRecord | null;
  isDecided: boolean;
  hasConsent: (category: ConsentCategory) => boolean;
  acceptAll: () => void;
  rejectAll: () => void;
  save: (decision: ConsentDecision) => void;
  openPreferences: () => void;
  closePreferences: () => void;
  preferencesOpen: boolean;
}

const ConsentContext = createContext<ConsentContextValue | null>(null);

export function ConsentProvider({ children }: { children: ReactNode }) {
  const [record, setRecord] = useState<ConsentRecord | null>(() => readConsent());
  const [preferencesOpen, setPreferencesOpen] = useState(false);

  function save(decision: ConsentDecision) {
    setRecord(writeConsent(decision));
    setPreferencesOpen(false);
  }

  const value: ConsentContextValue = {
    record,
    isDecided: record !== null,
    hasConsent: (category) => hasConsentInRecord(category, record),
    acceptAll: () => save(GRANTED_DECISION),
    rejectAll: () => save(DENIED_DECISION),
    save,
    openPreferences: () => setPreferencesOpen(true),
    closePreferences: () => setPreferencesOpen(false),
    preferencesOpen,
  };

  return <ConsentContext value={value}>{children}</ConsentContext>;
}

export function useConsent() {
  const ctx = useContext(ConsentContext);
  if (!ctx) throw new Error("useConsent must be used within ConsentProvider");
  return ctx;
}
