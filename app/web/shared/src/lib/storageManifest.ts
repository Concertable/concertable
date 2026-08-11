import type { ConsentCategory } from "@/lib/consent";

export type StorageApi =
  | "localStorage"
  | "sessionStorage"
  | "cookie"
  | "indexedDB";

/**
 * PECR/UK-GDPR classification. `necessary` and `functional` need no consent (still
 * disclosed); `analytics`/`marketing` load only behind a granted consent category.
 */
export type StorageClass =
  | "necessary"
  | "functional"
  | "analytics"
  | "marketing";

export interface StorageItem {
  key: string;
  api: StorageApi;
  owner: string;
  /** True when our own code performs the write (so the drift guard can find it). */
  firstParty: boolean;
  purpose: string;
  duration: string;
  classification: StorageClass;
  /** The consent category that must be granted before this item may be stored. */
  consentCategory?: ConsentCategory;
  /** SPAs that store it: customer | venue | artist | business. */
  apps: readonly string[];
  /**
   * First-party write locations, relative to `app/web`, one entry per write
   * occurrence. The drift-guard test asserts the code's actual storage writes
   * match exactly these — a new write fails until it is classified here.
   */
  writeSites?: readonly string[];
  notes?: string;
}

export const STORAGE_MANIFEST: readonly StorageItem[] = [
  {
    key: "cookie-consent",
    api: "localStorage",
    owner: "first-party",
    firstParty: true,
    purpose: "Records the user's cookie-consent decision so the banner is shown once.",
    duration: "Persistent until cleared",
    classification: "necessary",
    apps: ["customer", "venue", "artist", "business"],
    writeSites: ["shared/src/lib/consent.ts"],
  },
  {
    key: "theme",
    api: "localStorage",
    owner: "first-party",
    firstParty: true,
    purpose: "Remembers the light/dark UI preference.",
    duration: "Persistent until cleared",
    classification: "functional",
    apps: ["customer", "venue", "artist"],
    writeSites: ["shared/src/providers/ThemeProvider.tsx"],
  },
  {
    key: "concertable.active-tenant",
    api: "localStorage",
    owner: "first-party",
    firstParty: true,
    purpose:
      "Remembers the manager's selected active tenant so it persists across sessions.",
    duration: "Persistent until cleared",
    classification: "functional",
    apps: ["venue", "artist"],
    writeSites: [
      "b2b/shared/src/features/tenant/store/useTenantStore.ts",
    ],
    notes:
      "Written by the zustand persist middleware (default localStorage); key set via its name option.",
  },
  {
    key: "oidc.user:*, oidc.* state",
    api: "localStorage",
    owner: "oidc-client-ts (store configured by first-party)",
    firstParty: false,
    purpose: "Auth tokens and sign-in/silent-renew state for the logged-in session.",
    duration: "Session / token lifetime",
    classification: "necessary",
    apps: ["customer", "venue", "artist"],
    notes:
      "Written internally by oidc-client-ts via WebStorageStateStore in shared/src/features/auth/config/oidcConfig.ts; not an explicit setItem call.",
  },
  {
    key: "__stripe_mid",
    api: "cookie",
    owner: "Stripe",
    firstParty: false,
    purpose: "Stripe fraud-prevention (Radar) machine identifier during payment.",
    duration: "~1 year",
    classification: "necessary",
    apps: ["customer", "venue", "artist"],
    notes:
      "Set by Stripe.js, now loaded lazily at checkout (getStripe), so it fires only for a payment the user started.",
  },
  {
    key: "__stripe_sid",
    api: "cookie",
    owner: "Stripe",
    firstParty: false,
    purpose: "Stripe fraud-prevention (Radar) session identifier during payment.",
    duration: "~30 minutes",
    classification: "necessary",
    apps: ["customer", "venue", "artist"],
    notes: "Set by Stripe.js at checkout (see __stripe_mid).",
  },
  {
    key: "m",
    api: "cookie",
    owner: "Stripe (m.stripe.com)",
    firstParty: false,
    purpose: "Stripe fraud-prevention identifier.",
    duration: "~1-2 years",
    classification: "necessary",
    apps: ["customer", "venue", "artist"],
    notes: "httpOnly, set on m.stripe.com by Stripe.js at checkout.",
  },
] as const;
