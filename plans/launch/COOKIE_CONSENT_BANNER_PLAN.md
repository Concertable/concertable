# Cookie consent banner — PECR/UK-GDPR first-visit consent on the web SPAs

> Launch Swim-lane C scaffolding item (Month 2): a PECR/UK-GDPR-compliant cookie consent banner,
> universal UI in `app/web/shared`, mounted on every web SPA.
> **Next steps live in @plans/launch/COOKIE_CONSENT_BANNER_PROGRESS.md → `## Next Steps`** — this
> plan holds the design and outstanding phases only, no next-action prose.

## 1. Outcome

A first-visit consent banner on every web SPA that:

- prompts on first visit (no valid stored consent), before any non-essential cookie is set;
- keeps **strictly-necessary** cookies always on (auth/OIDC, the `sidebar:state` UI cookie, the
  `theme` preference, the consent record itself) with no toggle;
- offers **non-essential** categories (analytics, marketing) **opt-in, defaulted off**, with a
  **Reject all** control **as prominent as Accept all** (no dark pattern — PECR);
- sets **no** non-essential cookie before the corresponding category is granted;
- **persists** the choice and makes it **re-openable** ("Manage cookies") so consent can be changed
  or withdrawn;
- **links to** the (future) cookie/privacy policy route via a placeholder anchor.

This ships the banner **mechanics + persistence + categories with placeholder legal copy**. The
solicitor-drafted cookie/privacy text (Month 4) and the `/privacy` + `/cookies` page routes are
separate, still-blocked items; this feature does not wait on either — the real copy and real links
slot into the placeholders later.

## 2. Where it lives — universal UI in `app/web/shared`, router-free

The banner is genuinely universal (every SPA renders it, none can opt a visitor out of consent), so
by the web topology rules it belongs in the universal tier `app/web/shared` — never `b2b/shared`.
It compiles into all four apps, so **all four builds green is the boundary gate**.

**Hard constraint that shapes the whole design — the banner must be router-free.** One of the four
apps, **web-business** (`app/web/b2b/business`), is a minimal static SPA with **no TanStack Router
and no provider stack** — its `main.tsx` renders a plain gateway page. So the consent provider and
every banner component:

- use **no** router primitives (`<Link>`, `useNavigate`, `useRouterState`, route hooks);
- link to policies with a **plain `<a href="/cookies">`**, not `<Link to>`.

This constraint doubles as the fix for the placeholder-link problem: the policy routes don't exist
yet, and a typed `<Link to="/cookies">` to a non-existent route **fails the `tsc` route-tree check**
in the three router apps. A plain anchor to a not-yet-built path compiles everywhere and degrades to
a dead link until the route lands — exactly what "placeholder" needs.

### Surfaces — reconciling the roadmap's stale "3 SPAs" against the real four apps

The roadmap/checklist say "3 SPAs (customer/venue/artist)". That predates the **web-business**
gateway being carved out of the old landing app; the real surface set is **four**: `web-customer`,
`web-venue`, `web-artist`, `web-business`. Because the banner lives in `shared` it compiles into all
four regardless; it is **mounted (rendered) in all four**, justified per app:

- **web-venue, web-artist** — the core B2B v1 manager surfaces. In scope, unambiguous.
- **web-business** — the public **marketing/gateway** site (Checklist Phase 10) and the highest-
  traffic *unauthenticated* first-contact surface; it is also the UI-E2E landing page (`Login.feature`
  starts "on the business home page"). It must carry the banner, and it is the reason for the
  router-free constraint above.
- **web-customer** — the customer marketplace switch-on is deferred out of v1, but the app **still
  exists, still builds, and already sets OIDC auth cookies**. Mounting the banner is a one-line wire
  per app; doing it now means the eventual marketplace inherits PECR compliance for free instead of
  being a future gap. This is the scalable long-term choice over "remember to add it later" — the
  marginal cost is nil because `shared` compiles into customer anyway.

The §7 launch checkbox "live on all 3 SPAs" is satisfied by the three v1-served surfaces (venue,
artist, business); customer is covered too. Tick the roadmap in the four-app language on ship (§7 of
this plan).

## 3. Design — the vertical slice

New code is concentrated in `app/web/shared/src`; each app gets a small mount. Files, in dependency
order.

### Consent core — framework-free logic (`app/web/shared/src/lib/consent.ts`)

The pure record logic, no React, so it is unit-testable in a node vitest env (see §6 tests):

- **Types** — `ConsentCategory = "analytics" | "marketing"` (strictly-necessary is implicit, never a
  stored toggle); `ConsentDecision = Record<ConsentCategory, boolean>`; `ConsentRecord = { version:
  number; decidedAtUtc: string; categories: ConsentDecision }`.
- **Constants** — `CONSENT_STORAGE_KEY = "cookie-consent"` (mirrors the `theme` localStorage
  precedent); `CONSENT_VERSION` (integer, bumped when the policy/categories change materially to
  force a re-prompt); `DENIED_DECISION` = all categories `false` (the pre-consent / reject-all state).
- **Pure functions** — `readConsent(): ConsentRecord | null` (parse + validate; returns `null` on
  absent/corrupt/`version`-mismatch → re-prompt); `writeConsent(decision): ConsentRecord` (stamps
  `version` + `decidedAtUtc`, persists, returns it); `hasConsent(category, record?): boolean`
  (defaults to `readConsent()`; the single predicate every future non-essential script must call);
  `isDecided(record): boolean`.

`decidedAtUtc` uses `new Date().toISOString()` at call time (a real user event, not a workflow-script
context — the `Date.now` ban is a Workflow-script rule, irrelevant to app code).

### Consent provider + hook (`app/web/shared/src/providers/ConsentProvider.tsx`)

Mirror `ThemeProvider.tsx` exactly (the closest precedent — context + `useState` seeded from storage
+ setter persists):

- `ConsentProvider` holds `record: ConsentRecord | null` and `preferencesOpen: boolean`.
- `useConsent()` returns `{ record, isDecided, hasConsent, acceptAll, rejectAll, save(decision),
  openPreferences, closePreferences, preferencesOpen }`. `acceptAll`/`rejectAll`/`save` call
  `writeConsent` and update state; `hasConsent` reads current state.
- Also **subscribe seam** for imperative consumers: a module-level `onConsentChange(listener)` in
  `lib/consent.ts` that the provider fires on every write, so a future analytics loader mounted
  outside React can initialise the moment `analytics` flips true (and never before). Ship the seam
  now; there is no analytics tooling to wire yet.

### Banner + preferences UI (`app/web/shared/src/components/`)

Router-free, built on the existing design system (`ui/dialog`, `ui/checkbox`, `ui/button`,
`ui/label`, `ui/separator`):

- **`CookieConsentBanner.tsx`** — fixed bottom banner, rendered only when `!isDecided`. Short
  placeholder blurb + a plain `<a href="/cookies">` policy link, and three controls: **Accept all**,
  **Reject all**, **Manage cookies** (opens the dialog). Accept and Reject use **equal visual
  weight** (same button size/variant — not accept-primary/reject-ghost) to satisfy the PECR "as easy
  to reject as accept" rule. `data-testid`s for the E2E page object (`cookie-banner`,
  `cookie-accept-all`, `cookie-reject-all`, `cookie-manage`).
- **`CookiePreferencesDialog.tsx`** — `Dialog` driven by `preferencesOpen`. One row per category with
  a `Checkbox` (mirror the VAT-registered checkbox in `OrganizationForm.tsx`): **Strictly necessary**
  shown checked + **disabled** with an "always on" hint; **Analytics** and **Marketing** toggleable,
  default off. Footer: **Save preferences** (persists the current checkbox state) + **Reject all** +
  **Accept all**. `data-testid`s (`cookie-prefs`, `cookie-cat-analytics`, `cookie-cat-marketing`,
  `cookie-save-prefs`).
- **`ManageCookiesButton.tsx`** — a small `variant="link"` button calling
  `useConsent().openPreferences()`, for footers to re-open consent after dismissal.

### Footer re-open trigger + placeholder policy links (`app/web/shared/src/components/Footer.tsx`)

Add `<ManageCookiesButton/>` and placeholder `<a href="/cookies">Cookie policy</a>` /
`<a href="/privacy">Privacy</a>` anchors to the shared `Footer` (rendered by `AppLayout` in the three
router apps). The business app has its own inline footer — add the same manage-cookies trigger +
policy anchors there (it sits inside `<App/>`, which will be inside `<ConsentProvider>`).

### Mounts — one per app (`main.tsx` × 4)

Wrap each app's tree in `<ConsentProvider>` and render `<CookieConsentBanner/>` as a sibling of the
router/app root so it shows on **every** route including pre-login:

- **`app/web/customer/src/main.tsx`**, **`app/web/b2b/venue/src/main.tsx`**,
  **`app/web/b2b/artist/src/main.tsx`** — add `<ConsentProvider>` inside the existing provider stack
  (next to `ThemeProvider`) and `<CookieConsentBanner/>` beside `<RouterProvider>`.
- **`app/web/b2b/business/src/main.tsx`** — wrap `<App/>` in `<ConsentProvider>` and render
  `<CookieConsentBanner/>` as its sibling. This is the router-free path; if the banner accidentally
  imports a router primitive, **this build breaks first** — a useful guardrail.

## 4. Decisions

- **D1 — client-side persisted consent is the PECR record; no server-side proof for v1.** The banner
  fires on **first visit, pre-authentication**, when there is no user identity to key a server record
  to. ICO guidance treats a durable client-side record (categories + timestamp + policy version) as
  demonstrating cookie consent; a server-side consent ledger would require logging identifying data
  (IP/device) for **no** compliance gain and against data-minimisation. So the localStorage record
  **is** the proof. This is the ICO-aligned standard, **not** a shortcut. *Deferred, not v1:* if we
  ever link consent to a logged-in account (marketplace era), an authenticated server-side record can
  be added additively — logged as a future enhancement, not a gap.
- **D2 — localStorage, not a cookie, for the record.** Matches the `theme` precedent, and it is never
  transmitted, so it triggers no incidental server-side processing. The record is itself strictly-
  necessary/functional, so storing it pre-consent is permitted.
- **D3 — router-free by construction.** Forced by the business app (§2); also the correct way to hold
  a placeholder policy link that outlives a not-yet-existent route without breaking `tsc`.
- **D4 — three categories: necessary (always on) / analytics (off) / marketing (off).** The minimal
  PECR-correct split. No non-essential cookies exist in the codebase today (no analytics/marketing
  tooling is wired), so this feature ships the **gate**, not any actual gated script.
- **D5 — versioned consent with re-prompt.** `CONSENT_VERSION` bump invalidates stored consent and
  re-prompts — the mechanism that lets the Month-4 solicitor copy / category changes force fresh
  consent without a code migration.
- **D6 — test split follows CI reality (see §6).** Web vitest is **not** a merge-queue gate (CI
  builds the SPAs and runs UI E2E; it never runs web unit tests). So the pure consent logic is
  covered by **vitest unit tests** (local gate, mirroring the existing `b2b` `.test.ts` precedent),
  and the **rendered** accept/reject/manage flow is covered by **UI E2E** — the authoritative
  automated gate, and the only layer that renders the banner in a real browser. No jsdom/testing-
  library component-render harness is added: CI would not run it, and E2E covers the same ground for
  real. The absence of a web-vitest CI job is pre-existing test-infra debt, logged in
  `app/web/TECH_DEBT.md`, not fixed inside this feature PR.
- **D7 — mounted on all four SPAs.** Per §2; customer's inclusion is free future-proofing.

## 5. Out of scope

- Solicitor-drafted cookie/privacy **copy** (Month 4, blocked on solicitor) — placeholder text only
  here; the real text slots into the same components later.
- The **`/privacy` + `/cookies` page routes** — a separate blocked roadmap item; the banner links to
  them with placeholder anchors, does not build them.
- Any **analytics/marketing tooling** — the banner gates future non-essential cookies; there are none
  to wire yet. Adding an analytics SDK is a later change that must call `hasConsent("analytics")`.
- **Server-side consent record** (D1) — deferred marketplace-era enhancement.
- **Wiring web vitest into CI** (D6) — pre-existing test-infra debt, logged not fixed here.

## 6. Phases — one PR

No package publish, platform-sync, or runtime-deploy gate separates any of these (the feature is pure
`app/web/**`; the E2E scenario is test code). So it is **one coherent PR**; the phases are the
implementation ordering, each ending green.

- **Phase 1 — consent core + provider.** `lib/consent.ts`, `providers/ConsentProvider.tsx`,
  `useConsent`, `hasConsent`, the `onConsentChange` seam. Add a vitest config to `@concertable/web`
  (copy `app/web/b2b/venue/vitest.config.ts`, node env, `include` `src/**/*.test.ts`) + a `test`
  script, and `lib/consent.test.ts` covering read/write/version-invalidation/default-denied/
  `hasConsent`. **Green:** four builds + the new vitest suite.
- **Phase 2 — banner + preferences UI + footer trigger.** `CookieConsentBanner`,
  `CookiePreferencesDialog`, `ManageCookiesButton`; `Footer` gains the manage-cookies trigger +
  placeholder policy anchors. Equal-weight accept/reject. **Green:** four builds.
- **Phase 3 — mount + E2E.** Mount `<ConsentProvider>` + `<CookieConsentBanner/>` in all four
  `main.tsx`; add the business inline-footer trigger. Author a UI E2E `CookieConsent.feature` in
  `api/Concertable.B2B/tests/E2ETests/Concertable.B2B.E2ETests.Ui` (+ steps + page object, and
  reconcile `E2E_BASELINE.md`), starting on the business home page: banner shown on first visit →
  **Reject all** persists + dismisses + sets no non-essential cookie → reload keeps it dismissed →
  **Manage cookies** re-opens → toggle Analytics + **Save** persists; a second scenario for **Accept
  all**. **Green:** four builds; E2E runs in the merge queue.

### Verification gate

- All four web builds green — the boundary gate:
  `npm -w @concertable/web-customer run build`, `web-venue`, `web-artist`, `web-business`
  (build `@concertable/shared` first after a fresh `npm ci`; regenerate an app's `routeTree.gen.ts`
  only if routes changed — this feature adds none).
- The new `@concertable/web` vitest suite green (local): `npm -w @concertable/web run test`.
- **Merge-queue E2E tier: full E2E, do not skip.** The banner is universal `shared` UI mounted on
  every surface (including the E2E landing page) and changes a user-facing first-visit flow — it
  fails every `skip-e2e` criterion. Let the merge queue run E2E; do not duplicate it locally.

## 7. Delivery & close-out

- Open the PR with plain `gh pr create` (personal repo — no `AB#`, no assignee).
- **In the same commit as the feature**, report completion back to the roadmap: tick the §5 Swim-lane
  C row "Cookie consent banner on 3 SPAs (scaffolding)" (🟡/☐ → ✅) and the §7 checkbox "Cookie consent
  banner live on all 3 SPAs" (☐ → ✅), and correct the stale "3 SPAs" wording to the four apps. Leave
  the separate §5 "Cookie banner text … from solicitor → wired into banner" (Month 4) row untouched —
  that is the still-blocked copy item, not this scaffolding. Do not delete the roadmap.
- Merge via `/merge` (full E2E tier).
- **Platform-sync:** the feature touches no `api/**` service source, so no MinVer republish / no
  `chore/platform-sync-*` is expected. The E2E scenario adds test files under
  `api/**/tests/E2ETests` — if that trips the publish path-filter, a sync PR may open but is
  non-breaking (no published contract changed) and should auto-merge; own it to green regardless.
- **Close-out:** if no package/sync gate fires, the plan + ledger may close in the feature's final
  commit; otherwise close out after the sync is green. Either way `git rm` this plan and its
  `_PROGRESS.md` as a **doc-only close-out** riding the next change — never its own PR (`PLAN.md`
  Lifecycle 5 / doc-only close-out).
