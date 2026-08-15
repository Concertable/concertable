# app/web — Technical Debt

---

## MED

### Web concert detail Buy Tickets below `@3xl` — fixed, narrow-viewport E2E outstanding

Fixed on `Fix/TechDebtSweep`: the single `ConcertCard` now reflows (full-width at the top below
`@3xl`, sticky sidebar at/above it) instead of being `display:none`, so `buy-tickets` is reachable at
every width and stays one unambiguous testid (Playwright strict mode stays happy). Outstanding only: a
**narrow-viewport E2E** asserting `buy-tickets` is reachable at a sub-`@3xl` width (needs Docker).

**Resolves when:** the narrow-viewport E2E scenario lands green.

### Browser-storage classification is detection-by-regex, not prevention-by-construction

First-party device storage has no single sanctioned accessor: `consent.ts` (`cookie-consent`),
`ThemeProvider` (`theme`), and `useTenantStore` (zustand `persist` → `concertable.active-tenant`) each
write `localStorage` their own way. The "new storage must be classified" guarantee is enforced by a
**regex drift-guard** (`shared/src/lib/storageManifest.test.ts`) that scans for known write patterns
(`setItem`, `document.cookie=`, `persist(`, `indexedDB.open(`) against `STORAGE_MANIFEST` — detection
after the fact, not prevention. This already missed once: the guard was blind to zustand `persist()`'s
implicit write, so `concertable.active-tenant` shipped unclassified and undetectable until code review
caught it (finding NAT1). A novel first-party write mechanism can slip past the same way.

Durable fix: a generic first-party `createClassifiedStorage({ key, api, classification,
consentCategory })` in `shared/src/lib` that is the only sanctioned way our code touches storage —
it auto-registers itself in `STORAGE_MANIFEST` and refuses to write an `analytics`/`marketing` item
until `hasConsent(category)`. `consent.ts` and `ThemeProvider` move onto it; classification becomes a
compile/construction property instead of a scanner's guess. **Caveat (why the manifest + drift-guard
stay):** third-party writers we don't control — oidc-client-ts, Stripe.js, and zustand `persist`
itself — always write on their own, so they can never route through the accessor; the manifest + guard
remain the catch-all for those. This hardens only the first-party path.

**Resolves when:** first-party storage writes go through the classified accessor and the drift-guard's
role is reduced to covering the enumerated third-party/library writers.

---

### The customer SPA mounts the Mailbox against an endpoint its backend does not have

`Navbar.tsx` renders `{user && <Mailbox />}` and lives in `app/web/shared` — the universal tier every
SPA compiles — so the customer app mounts it for any signed-in user. `useMailbox` fires
`useUnreadCountQuery` on mount, which calls `/message/user/unread-count` on the own-site `apiClient`;
for the customer app that is the Customer service, which has **no `MessageController` at all**. So every
customer page load makes a request that 404s, and the bell renders for a product with no messaging.

Found while adding the Online Safety Act report control to the same component; it predates that work.

**Resolves when:** the Mailbox is injected by the manager apps rather than declared in the universal
Navbar (matching how `app/web/shared/AGENTS.md` says app-specific affordances are composed — a slot the
owning app fills), or messaging genuinely ships on the customer side. A role check inside shared code is
explicitly not the fix — that is the disease that doc warns about.
