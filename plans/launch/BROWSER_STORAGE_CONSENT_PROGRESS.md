# Browser-storage audit + consent correction — progress

- Plan: `plans/launch/BROWSER_STORAGE_CONSENT_PLAN.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\launch_browser-storage-consent`
- Branch: `Feature/launch_browser-storage-consent`
- PR: not opened
- Dependency/package gates: **no hard platform-sync gate** — the only `api/**` edits will be E2E test
  projects, so no published `Concertable.*` package changes shape; any `chore/platform-sync-*` PR is
  non-breaking and auto-merges. **Legal input (non-blocking):** solicitor-drafted cookie/storage
  **policy copy** is a Month-4 input and gates ONLY the final policy-wording wire (Phase 4 tail), not
  the audit/removal/correction/inventory work.
- Last reconciled: 2026-08-10 — **Phase 1 COMPLETE**: static sweep + anonymous runtime capture across
  all four SPAs (real headless Chromium, storage dumped per journey). Synced to `origin/main` at start
  (was 2 behind; merged, clean). Worktree at branch tip; tree clean (no code changed).

## Current state

**Phase 1 is complete and the decision gate is open (recorded below).** Static inventory + the
anonymous runtime capture are both done; the capture **overturned a static assumption** (Stripe is
*not* lazy — see the discovery below), which is exactly why the audit-first ordering mattered. Per the
agreed capture scope (Tommy, this session), the authenticated + Stripe-checkout journeys were **not**
booted: they need the full Aspire/Docker backend + seeded accounts and would only refine exact
third-party key strings that are classified necessary/exempt by *purpose* regardless — their storage
is documented from authoritative library behaviour and flagged as documented-not-observed. No `app/web`
or test code changed — the four SPA builds remain at the `origin/main` baseline. **Phase 2 (removal)
is now unblocked.**

Branch-time gate at creation: 0 open red `chore/platform-sync-*` PRs; `origin/main` carried the
reopened roadmap line and the merged `Docs/launch_cookie-storage-audit` PR #469.

## Next Steps

Phase 1 is done and the decision gate (below) is set. Start **Phase 2 — remove what the audit shows
is unjustified**, each removal citing its inventory line in the commit message:

1. **Remove the write-only `sidebar_state` cookie** — delete the `document.cookie` write in
   `app/web/shared/src/components/ui/sidebar.tsx:85` (never read; static-definitive). Decide from
   evidence whether sidebar-open persistence is even wanted; the runtime capture gives no reason to
   re-add it, so drop persistence unless Tommy wants it.
2. **Remove the decorative consent machinery** — the analytics/marketing banner gates nothing and no
   such technology loads anywhere (runtime-confirmed: accepting set no new storage; zero analytics
   hosts). Remove `consent.ts` + `consent.test.ts`, `ConsentProvider.tsx`, `CookieConsentBanner.tsx`,
   `CookiePreferencesDialog.tsx`, `ManageCookiesButton.tsx`, the wiring in all four `main.tsx`, the
   `Footer.tsx` "Manage cookies" affordance, **and** the E2E/test scaffold that exists only to
   suppress it (`CookieConsent.feature`, `CookieConsentPage.cs`, `CookieConsentSteps.cs`,
   `CookieConsentState.cs`, and the `establishDeniedCookieConsent` plumbing in both B2B + Customer
   `Browser.cs`). **Precondition:** this is correct **only because Phase 3 replaces consent with
   lazy/functional-on-use loading** (see decision gate) — the real non-exempt boot-time third parties
   (Stripe, Maps) are handled by not-loading-until-used, not by a banner. If the solicitor later rules
   lazy Stripe/Maps still need a consent gate, that gate is built fresh in Phase 3 against the manifest
   (it must gate *those* technologies, never phantom analytics/marketing) — so keep the removal and the
   Phase 3 gate design separate.
3. Gate each removal on: four SPA builds green; shared vitest green (with `consent.test.ts` gone); the
   B2B + Customer UI E2E projects compile and their fixtures no longer reference removed symbols.
4. Then **Phase 3** (lazy Stripe init + lazy Maps + the durable storage manifest/drift-guard) and
   **Phase 4** (engineering inventory doc). Phase 3 scope now includes **Stripe** — see the discovery.

**Re-running the runtime capture** (for Phase 2/3 verification): `npm install` in `app/`, then
`npm run build:web-packages`, then `npm run dev:customer|venue|artist|business` from `app/`; drive with
headless Chromium (`ms-playwright/chromium_headless_shell-1228`, launch args `--no-sandbox --disable-gpu
--disable-dev-shm-usage`, `ignoreHTTPSErrors`, block `localhost:7083/7087/7088/7090` so the SPA can't
hang on the dead OIDC/API hosts). The capture script + method are captured in this session's evidence.

## Completed work

- **Worktree + branch** `Feature/launch_browser-storage-consent` off `origin/main` (this session).
- **Plan + ledger authored** — `plans/launch/BROWSER_STORAGE_CONSENT_PLAN.md` + this file (committed
  with this ledger update).
- **Phase 1 static inventory** — complete code sweep of `app/web/**` and the E2E consent machinery
  (evidence below).
- **Phase 1 runtime capture (anonymous)** — booted all four SPAs (`npm run dev:*` after
  `build:web-packages`) and drove customer `/` + `/find`, venue `/`, artist `/`, business `/` through a
  real headless Chromium, dumping cookies + localStorage + sessionStorage + IndexedDB per journey
  (evidence + decision gate below). Overturned the static "Stripe is lazy" assumption.

## Phase 1 audit — static inventory (evidence, from code)

### First-party storage (set by our own code)

| Item | API | Where set | Read back? | SPAs | Purpose | Provisional class |
|---|---|---|---|---|---|---|
| `oidc.user:<authority>:<client_id>` + oidc state keys | localStorage | `shared/src/features/auth/config/oidcConfig.ts` (`WebStorageStateStore({ store: window.localStorage })`, `automaticSilentRenew`) | yes (auth/session, silent renew) | customer, venue, artist | Auth tokens + session state | **Necessary (exempt)** |
| `cookie-consent` | localStorage | `shared/src/lib/consent.ts` `writeConsent` | yes (`readConsent`) | all four | Stores the consent decision itself | **Necessary (exempt) — but only meaningful if consent is actually required** |
| `theme` | localStorage | `shared/src/providers/ThemeProvider.tsx` | yes | customer, venue, artist (ThemeProvider not mounted in business) | UI light/dark preference | **Functional** (no PII; user preference) — *not in the kickoff brief; discovered in the sweep* |
| `sidebar_state` | cookie (`document.cookie`, `path=/`, `max-age`=7d) | `shared/src/components/ui/sidebar.tsx:85` (every `setOpen`) | **NO — never read anywhere** (`defaultOpen` is a literal `true`; no `document.cookie` read exists in `app/web`) | wherever `Sidebar` renders (manager apps) | Intended sidebar-open persistence; a shadcn/SSR idiom with no SSR in a client-only SPA | **Unused → remove** |

### Third-party technologies the SPAs load (set their own storage)

| Tech | Loaded where | When | Storage it sets | Provisional class |
|---|---|---|---|---|
| **Stripe.js** (`@stripe/stripe-js` `loadStripe`) | `shared/src/lib/stripe.ts`; used by `StripePaymentForm`, `NewCardSection`, `handle3ds.ts` on customer `TicketCheckoutPage`, venue `VenueAcceptCheckoutPage`, artist `ArtistApplyCheckoutPage` | **lazy** — only when a checkout/payment component mounts | `__stripe_mid` (~1yr), `__stripe_sid` (~30min) fraud-prevention cookies (confirm names/durations at runtime) | **Necessary for payment/fraud (exempt)** — scoped to checkout the user requested |
| **Google Maps JS API** (`@vis.gl/react-google-maps` `APIProvider`) | `MapsProvider` in `main.tsx` of customer, venue, artist; consumed by `SearchBar` (places) + `GoogleMap` | **at app boot, unconditionally** (before any map/search is shown) | Google may set cookies/localStorage (capture at runtime) | **Non-exempt optional CANDIDATE — the real consent question.** Loaded-at-boot is the PECR concern; functional-on-use if made lazy |

### Consent machinery reality (the decisive finding)

- The banner (`CookieConsentBanner.tsx`) + dialog (`CookiePreferencesDialog.tsx`) offer **Analytics**
  and **Marketing** toggles, persisted by `consent.ts`, exposed via `ConsentProvider`.
- `hasConsent(category)` and `onConsentChange(...)` have **zero production consumers** — grep across
  `app/` shows the only references are the provider re-export, the consent UI (which reads only
  `isDecided`/`acceptAll`/`rejectAll`/`openPreferences`/`save`/`record`), and `consent.test.ts`.
  **Nothing loads a script based on the decision.**
- **No analytics/marketing technology exists in any SPA.** Grep for `gtag|googletagmanager|
  google-analytics|plausible|posthog|mixpanel|segment|hotjar|fbq|clarity|amplitude|VITE_(GA|GTM|…)`
  → zero real hits (only "segment"/"tostringtag" text noise). The four `index.html` files contain no
  analytics/script tags — just the module entry.
- ⇒ **The generic consent machinery gates nothing; it is decorative.** The one genuinely non-exempt
  third-party (Google Maps at boot) is **not** gated by it. So the banner is both decorative *and*
  mis-targeted.

### E2E / test machinery tied to consent (blast radius if the banner is removed)

- `api/…/Concertable.B2B.E2ETests.Ui/Features/CookieConsent.feature` — 2 scenarios, both on the
  **business** home page (reject-all persists + reopen; accept-all grants).
- `.../PageObjects/CookieConsentPage.cs`, `.../Steps/CookieConsentSteps.cs` — the step
  `no non-essential cookies are stored` asserts absence of `_ga/_gid/_gat/_fbp/_hj/ajs_/mp_` prefixes;
  it passes trivially because nothing ever sets them (a guard against tech that was never added).
- `api/Concertable.Shared/tests/Concertable.E2ETests/Support/CookieConsentState.cs` —
  `EstablishDeniedAsync` init-script that pre-seeds a denied consent record so the banner doesn't block
  other scenarios. Consumed by **both** `Browser.cs` in B2B UI (`:53`) and Customer UI (`:37`) via an
  `establishDeniedCookieConsent` flag on `InitializeAsync`.
- `shared/src/lib/consent.test.ts` — the shared unit test for the consent record.
- **Customer SPA has no dedicated consent E2E** — it mounts the banner (customer `main.tsx`) but the
  customer UI suite only carries the `establishDeniedCookieConsent` suppression scaffold, no consent
  test of its own.

### SPA wiring summary

- customer / venue / artist `main.tsx`: `ThemeProvider > ConsentProvider > (RouterProvider + CookieConsentBanner)`, inside `AuthProvider` (OIDC) + `MapsProvider` (Google Maps).
- business `main.tsx`: static marketing gateway — `ConsentProvider > (App + CookieConsentBanner)` only; **no** auth, router, theme, Maps, or Stripe. Its hand-rolled footer shows `ManageCookiesButton` + `/cookies` + `/privacy` links (those routes don't exist yet — separate Month-4 item). So on business the banner asks for analytics/marketing consent while the site stores only its own `cookie-consent` record.

## Phase 1 audit — runtime capture (evidence, from a real browser)

Method: headless Chromium (`ms-playwright/chromium_headless_shell-1228`, `--no-sandbox --disable-gpu
--disable-dev-shm-usage`, `ignoreHTTPSErrors`), one fresh context per journey, requests to the
non-running OIDC/API hosts (`localhost:7083/7087/7088/7090`) aborted so the SPA renders its boot state
instead of hanging on a dead redirect. Google + Stripe were **not** blocked. Dumped
cookies + localStorage + sessionStorage + IndexedDB after each load; on customer `/` also after
clicking **Accept all**. Scope: anonymous journey only (per agreed capture scope — see Current state).

### What actually loaded / was stored

| Journey | Third-party hosts contacted **at boot** | Device storage set (observed) |
|---|---|---|
| customer `/` (anon, pre-consent) | `js.stripe.com`, `m.stripe.com`, `m.stripe.network`, `r.stripe.com`, `maps.googleapis.com` | **Stripe cookies, set immediately:** `__stripe_mid` (localhost, ~5 yr, Strict), `__stripe_sid` (localhost, ~30 min, Strict), `m` (m.stripe.com, ~5 yr, httpOnly). **No** localStorage, sessionStorage, or IndexedDB. |
| customer `/find` (anon) | same as above | same Stripe cookies; **no** Maps/OIDC/other storage |
| customer `/` **after Accept-all** | (no new hosts) | **only** `cookie-consent` localStorage appears: `{"version":1,…,"categories":{"analytics":true,"marketing":true}}`. **No new cookies/storage** — accepting gated nothing. |
| venue `/` (anon, manager) | `js.stripe.com`, `maps.googleapis.com` | **none** (Stripe host contacted at boot but no cookie observed pre-redirect) |
| artist `/` (anon, manager) | `js.stripe.com`, `maps.googleapis.com` | **none** |
| business `/` (static) | **none** | **none** — loads zero third parties; banner is present but stores nothing on load |

### Runtime findings (what the capture proved that static analysis could not)

1. **Stripe.js is eager, not lazy — and this overturns the static inventory.** On the **anonymous**
   customer landing page, before any consent and with no checkout in sight, Stripe.js loads and sets
   `__stripe_mid`/`__stripe_sid`/`m`. Root cause is code-confirmed: `app/web/shared/src/lib/stripe.ts`
   calls `loadStripe(...)` at **module top-level** (`export const stripePromise = loadStripe(...)`), so
   Stripe.js fires the instant any importing module enters the boot graph (it does, on customer/venue/
   artist). A persistent `__stripe_mid` (~5 yr) set during anonymous browsing **weakens the "strictly
   necessary for a payment the user requested" exemption**, because at boot no payment is requested.
2. **Google Maps loads at boot but sets no observed device storage.** `maps.googleapis.com` is
   contacted at app boot (via `MapsProvider`), but the capture found **no** Maps cookie/localStorage/
   sessionStorage/IndexedDB. The PECR concern for Maps is therefore the **third-party script contact at
   boot**, not stored data — lower storage-impact than assumed, still a lazy-load candidate.
3. **The consent banner gates nothing — confirmed at runtime.** Accepting analytics+marketing wrote
   only the `cookie-consent` record and loaded/stored nothing else; the Stripe cookies were already
   set regardless of the decision. Decorative, as the static grep suggested.
4. **No analytics/marketing technology exists — confirmed.** Across every SPA the only third-party
   hosts are Stripe and Google Maps. Zero analytics/tag hosts.
5. **IndexedDB and sessionStorage are unused everywhere.** Nothing set them in any journey.
6. **No OIDC localStorage keys anonymously.** `oidc.user:*` is written only after a real login (not
   exercised in this anonymous capture); documented from `oidc-client-ts` behaviour as necessary/exempt.

## Phase 1 decision gate (classified inventory → phase assignment)

| Item | Owner | Final class | Phase | Basis |
|---|---|---|---|---|
| `sidebar_state` cookie | first-party | **Unused** (write-only, never read) | **2 — remove** | static: no read path in a client SPA |
| consent machinery + analytics/marketing categories (`consent.ts`, provider, banner, dialog, `ManageCookiesButton`, all `main.tsx` wiring, `Footer` affordance, E2E suppression scaffold) | first-party | **Gates nothing / decorative** | **2 — remove** | runtime: accept set no storage; zero analytics tech |
| Stripe.js eager `loadStripe` at module top-level → `__stripe_mid`/`__stripe_sid`/`m` | third-party (Stripe) | **Non-exempt at boot** (loads before any payment) | **3 — make lazy** (init at checkout only) | runtime: cookies set on anonymous landing |
| Google Maps JS at boot (`maps.googleapis.com`) | third-party (Google) | **Non-exempt optional, loads at boot** (no stored data) | **3 — lazy-load / functional-on-use** | runtime: contacted at boot, sets no storage |
| durable storage manifest + drift-guard test | first-party (new) | governance | **3 — build** | plan's real deliverable |
| `oidc.user:*` + OIDC state (localStorage) | first-party (oidc-client-ts) | **Necessary (exempt)** — auth | **4 — document** | purpose; documented-not-observed (no login exercised) |
| `theme` (localStorage) | first-party | **Functional** | **4 — document** (retain) | user preference, no PII |
| Stripe cookies **once lazy + checkout-scoped** | third-party (Stripe) | **Necessary (exempt)** — payment/fraud | **4 — document** | after Phase 3 makes them fire only at checkout |
| `cookie-consent` record | first-party | consent record itself | n/a | removed with the banner in Phase 2 (no consent decision to store once the decorative banner is gone) |

**Decision gate summary.** The real non-exempt, boot-time third parties are **Stripe and Google Maps**
— *not* the analytics/marketing the banner asks about. The durable, plan-preferred fix is
**functional-on-use / lazy loading** for both (load Stripe only at checkout; load Maps only on the
find/autocomplete routes), which removes all non-essential boot-time third-party contact and makes the
decorative banner removable outright (Phase 2). The **only** question left for the solicitor is narrow:
whether lazy-loaded, use-scoped Stripe/Maps still require a consent gate on top of being loaded only
on use — if yes, Phase 3 builds that gate fresh against the manifest, gating *those* technologies
(never phantom analytics/marketing). This is a narrowing of the Month-4 legal input, not a blocker on
Phases 2–4.

## Verification

Runtime capture ran green (all five journeys, no page errors). **No `app/web` or test code changed** —
the four SPA builds remain at the `origin/main` baseline; the worktree tree is clean (dev-server route
tree regenerations were reverted). Build/test verification of code changes begins with Phase 2's first
change.

## Reviews

None yet.

## Decisions, discoveries, blockers, and deviations

- **Discovery (runtime, headline) — Stripe.js is eager, not lazy.** `lib/stripe.ts` calls `loadStripe`
  at module top-level, so Stripe.js loads and sets `__stripe_mid`/`__stripe_sid`/`m` on the **anonymous
  customer landing page**, before any consent or checkout. This **overturns the static inventory's
  "Stripe is lazy / exempt" line** and promotes eager Stripe to a **Phase 3** lazy-init task, not just a
  Phase 4 "document as necessary" item. The single most valuable thing the runtime capture found.
- **Discovery — banner gates nothing (runtime-confirmed).** Accepting analytics+marketing wrote only
  the `cookie-consent` record and loaded/stored nothing else; zero analytics hosts anywhere. The static
  hypothesis is now proven. Removal is the Phase-2 branch (safe because Phase 3 replaces it with
  lazy/on-use loading of the real third parties).
- **Refinement — Google Maps sets no device storage (runtime).** Maps is contacted at boot but stored
  nothing observable; the PECR concern is the boot-time script contact, not stored data. Still a
  lazy-load candidate.
- **Deviation — capture scope (Tommy, this session).** Only the **anonymous** journey was booted with a
  real browser; the authenticated + Stripe-checkout journeys were not (they need the full Aspire/Docker
  backend + seeded accounts and would only refine exact key strings classified necessary/exempt by
  purpose regardless). Their third-party storage (`oidc.*`, checkout-time Stripe cookies) is documented
  from library behaviour, flagged documented-not-observed. Rationale recorded in Current state.
- **Discovery — `theme` localStorage** not named in the kickoff brief; folded into the inventory as
  functional storage.
- **Discovery — Google Maps at boot** is the only real non-exempt optional candidate, and it is
  currently *un*gated. This, not analytics, is where a consent-before-load (or lazy-load) design
  should focus if any consent mechanism is kept.
- **Discovery — `sidebar_state` cookie is write-only** (no read path in a client SPA). Unused → remove.
- **Deviation from a naive reading of the brief:** the brief lists the sidebar cookie + Auth + Stripe;
  the sweep adds `theme` and Google Maps, and confirms the customer SPA has no consent E2E — exactly
  why the audit must not trust the starting list.
- **No hard platform-sync gate** — the `api/**` edits are test-only; documented above.

## Event log

### 2026-08-10 — worktree created, plan + ledger authored, Phase 1 static audit done

- Action: Created worktree/branch `Feature/launch_browser-storage-consent` off `origin/main`
  (branch-time gate clean: 0 red platform-sync PRs). Wrote the plan and this ledger. Completed the
  Phase 1 **static** inventory via a full `app/web/**` storage sweep and an E2E-machinery sweep.
- Evidence: HEAD `c7968828e`; greps for storage APIs, consent consumers, and analytics loaders (all
  recorded in the inventory tables above); reads of `consent.ts`, `ConsentProvider.tsx`, the consent
  UI, `oidcConfig.ts`, `ThemeProvider.tsx`, `sidebar.tsx`, `stripe.ts`, `StripePaymentForm.tsx`, all
  four `main.tsx`/`index.html`, and the four E2E consent files + both `Browser.cs`.
- Outcome: Decisive static picture — decorative + mis-targeted consent UI; write-only sidebar cookie;
  necessary Auth/Stripe storage; Google Maps as the one real consent candidate. Runtime capture
  remains to confirm third-party storage and finalise classification.
- Follow-up: Phase 1 step 2 (runtime capture) per `## Next Steps`, then the decision gate.

### 2026-08-10 — Phase 1 runtime capture done; decision gate opened

- Action: Synced worktree to `origin/main` (was 2 behind; clean merge). Installed the `app/` web
  workspace, built the web packages (`build:web-packages`), booted all four SPA dev servers, and drove
  the **anonymous** journeys (customer `/` + `/find`, venue `/`, artist `/`, business `/`) through a
  real headless Chromium, dumping cookies + localStorage + sessionStorage + IndexedDB per journey (plus
  a post-"Accept all" dump on customer `/`).
- Evidence: capture table + runtime findings above; `lib/stripe.ts` read to confirm the eager
  `loadStripe` root cause. Anonymous journey only, per agreed scope.
- Outcome: **Phase 1 complete.** Key discovery — Stripe.js is eager (loads on anonymous landing, sets
  `__stripe_mid`/`__stripe_sid`/`m`), overturning the static "lazy" assumption. Maps loads at boot but
  stores nothing. Banner gates nothing (accept set no new storage); zero analytics anywhere; IndexedDB/
  sessionStorage unused. Decision gate recorded: sidebar cookie + decorative consent machinery → Phase
  2 remove; eager Stripe + boot-time Maps → Phase 3 lazy/on-use; OIDC/theme/lazy-Stripe → Phase 4
  document. Tree clean (dev-server route-tree regenerations reverted); builds at baseline.
- Follow-up: Phase 2 removal per the updated `## Next Steps`.

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\launch_browser-storage-consent
Read @plans/launch/BROWSER_STORAGE_CONSENT_PLAN.md and @plans/launch/BROWSER_STORAGE_CONSENT_PROGRESS.md, then do what the ledger's `## Next Steps` says.
```
