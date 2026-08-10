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
- Last reconciled: 2026-08-10 — **Phases 1–4 engineering COMPLETE.** Synced to `origin/main` (was 3
  behind; clean merge). Shipped the consent gating primitive, lazy Stripe (prior), on-use Maps, the
  storage manifest + drift-guard test, and the `BROWSER_STORAGE.md` inventory. Four SPA builds green;
  shared vitest 19/19 green. Pushed to the feature branch; PR not yet opened (opt-in).

## Current state

**Engineering for the whole plan is complete (Phases 1–4).** The banner is retained and its
Analytics/Marketing toggles now genuinely gate loading via the new primitive; the two boot-time third
parties (Stripe, Maps) load on use only; every stored item is classified in a drift-guarded manifest
and documented in the engineering inventory. Four SPA builds green, shared vitest 19/19 green, tree
clean, commits pushed to `Feature/launch_browser-storage-consent`.

Delivered this session (all committed + pushed):
- `lib/consentGate.ts` — `registerConsentGated` + `registerConsentGatedScript` (7 unit tests). The
  integration point future GA4/pixels register against; makes `hasConsent` actually gate.
- On-use Maps — root `APIProvider` removed from all three `main.tsx`; scoped
  `providers/MapsProvider.tsx` mounted around `FindPage` (search/autocomplete) and `GoogleMap`
  (detail maps). Maps no longer contacts Google on landing/home/b2b find lists.
- `lib/storageManifest.ts` + `storageManifest.test.ts` — classified single source of truth + a
  drift-guard that fails the build on any unclassified `localStorage`/`sessionStorage`/cookie/
  IndexedDB write in `app/web`.
- `app/web/shared/BROWSER_STORAGE.md` — the engineering inventory the solicitor drafts policy from.
- Lazy Stripe (`getStripe()`) shipped last session (`de4cda25f`).

**Remaining is delivery + legal, not engineering:** review → PR → merge (full E2E tier) → follow the
`chore/platform-sync-*` PR to green; then the solicitor policy-copy wire (Month-4, gated on legal
input) and the Maps consent-gate variant (solicitor call). Roadmap line + §7 checklist ticks land at
merge close-out (they signal launch-readiness, which needs the merge + legal tail).

Branch-time gate at creation: 0 open red `chore/platform-sync-*` PRs; `origin/main` carried the
reopened roadmap line and the merged `Docs/launch_cookie-storage-audit` PR #469.

## Next Steps

All engineering is done, committed, and pushed. What remains is delivery + a documented legal tail.

1. ✅ **Review the branch — DONE (2026-08-10).** Full code review over `origin/main..HEAD`; one finding
   (NAT1 — drift guard blind to zustand `persist()`) found and fixed + verified (vitest 19/19, four
   builds green). Work-order: `reviews/Feature-launch_browser-storage-consent.md`. Fix committed; branch
   synced to `origin/main`. **Not yet pushed** (push awaits go-ahead per repo rule).
2. **Open the PR + merge** (awaits Tommy's go-ahead — PR creation is opt-in). Use `/merge` at the
   **full E2E tier** — this changes a first-visit flow across all SPAs (banner + boot-time script
   loading), so do **not** `skip-e2e`. Let the merge queue run E2E; don't duplicate it locally.
3. **Follow the platform-sync PR** — the `api/**` edits are E2E-test-only, so the
   `chore/platform-sync-*` PR is non-breaking and auto-merges; confirm it greens.
4. **At merge close-out:** tick the roadmap line (`plans/launch/LAUNCH_ROADMAP.md:30`, `:197`) and the
   §7 checklist items (`plans/launch/LAUNCH_CHECKLIST.md:41`, `:42` — the `[CODE]` parts) in the
   close-out commit; move recovery state to a `Docs/*_closeout` worktree and delete this plan + ledger.

**Legal-gated tail (not blocking the above):**
- Solicitor drafts cookie/storage policy copy from `app/web/shared/BROWSER_STORAGE.md`; wiring it into
  the `/cookies` page is the only remaining step (that route is the separate Month-4 launch item).
- Whether Google Maps must sit behind a `functional` consent category off its core-search pages — a
  solicitor/product call. Maps already loads on-use; the gate variant just wraps the `MapsProvider`
  mount points with `registerConsentGated`, no re-architecture.

**Re-running the runtime capture** (optional verification of the on-use Maps / lazy-Stripe fixes): from
`app/`, `npm install` → `npm run build:web-packages` → `npm run dev:customer|venue|artist|business`;
drive with headless Chromium (`ms-playwright/chromium_headless_shell-1228`, args `--no-sandbox
--disable-gpu --disable-dev-shm-usage`, `ignoreHTTPSErrors`, block `localhost:7083/7087/7088/7090`).
Expect: customer `/` and b2b find lists contact **neither** `maps.googleapis.com` nor `js.stripe.com`.

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
- **Phase 2 (reduced)** — dead `sidebar_state` cookie removed (`2c31f44da`); consent machinery retained.
- **Phase 3** — lazy Stripe `getStripe()` (`de4cda25f`); consent gating primitive `lib/consentGate.ts`
  + 7 tests (`909af3791`); on-use Maps via scoped `providers/MapsProvider.tsx`, root `APIProvider`
  removed from all three `main.tsx` (`a43e334cc`); storage manifest + drift-guard test (`86e4fba78`).
- **Phase 4** — `app/web/shared/BROWSER_STORAGE.md` engineering inventory (mirrors the manifest).

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
| consent machinery + analytics/marketing categories (`consent.ts`, provider, banner, dialog, `ManageCookiesButton`, all `main.tsx` wiring, `Footer` affordance, E2E suppression scaffold) | first-party | **RETAIN — infra ahead of roadmapped analytics/marketing** (was "decorative → remove"; corrected — see Next Steps) | **3 — make it actually gate** | analytics/marketing tracking is on the commercial roadmap; UK PECR requires the banner once it loads; peer marketplaces ship the same |
| Stripe.js eager `loadStripe` at module top-level → `__stripe_mid`/`__stripe_sid`/`m` | third-party (Stripe) | **Non-exempt at boot** (loads before any payment) | **3 — make lazy** (init at checkout only) | runtime: cookies set on anonymous landing |
| Google Maps JS at boot (`maps.googleapis.com`) | third-party (Google) | **Non-exempt optional, loads at boot** (no stored data) | **3 — lazy-load / functional-on-use** | runtime: contacted at boot, sets no storage |
| durable storage manifest + drift-guard test | first-party (new) | governance | **3 — build** | plan's real deliverable |
| `oidc.user:*` + OIDC state (localStorage) | first-party (oidc-client-ts) | **Necessary (exempt)** — auth | **4 — document** | purpose; documented-not-observed (no login exercised) |
| `theme` (localStorage) | first-party | **Functional** | **4 — document** (retain) | user preference, no PII |
| Stripe cookies **once lazy + checkout-scoped** | third-party (Stripe) | **Necessary (exempt)** — payment/fraud | **4 — document** | after Phase 3 makes them fire only at checkout |
| `cookie-consent` record | first-party | consent record itself | n/a | removed with the banner in Phase 2 (no consent decision to store once the decorative banner is gone) |

**Decision gate summary (CORRECTED).** Two independent things were conflated in the first draft:
(1) the boot-time third parties **Stripe and Google Maps** load before consent/use — the durable fix is
**lazy / functional-on-use loading** (Stripe only at checkout; Maps only on find/autocomplete routes),
which is unchanged and correct; and (2) the **analytics/marketing consent banner**, which the first draft
wrongly concluded should be deleted. It is **retained**: analytics/marketing tracking is roadmapped, peer
UK marketplaces ship the same banner, and UK PECR mandates it once that tech loads. The banner's genuine
defect is that its toggles gate nothing — Phase 3 makes them **actually gate** (consent-before-load keyed
off `hasConsent`), and decides custom-banner-vs-CMP + exact categories from the competitor/legal research
now in flight. Neither the solicitor input nor the tracking-vendor choice blocks the engineering: the
gating primitive + lazy Stripe/Maps + manifest are built now; wiring a specific analytics vendor and the
final policy copy are the tail.

## Verification

Runtime capture ran green (all five journeys, no page errors). **No `app/web` or test code changed** —
the four SPA builds remain at the `origin/main` baseline; the worktree tree is clean (dev-server route
tree regenerations were reverted). Build/test verification of code changes begins with Phase 2's first
change.

## Reviews

- **2026-08-10 — full code review** (`reviews/Feature-launch_browser-storage-consent.md`), two layers
  (native `code-reviewer` subagent + architecture lenses) over `origin/main..HEAD`. Security layer
  skipped — no path matches the merge gate's `_SECURITY_PATTERNS` (pure frontend). **One finding, fixed:**
  **NAT1 (MEDIUM)** — the drift guard was blind to zustand `persist()` stores, so
  `concertable.active-tenant` (persisted by `b2b/shared/…/useTenantStore.ts` in venue+artist) was
  unclassified and undetectable, falsifying the "new storage can't ship unclassified" guarantee. Fixed:
  added a `persist(` write-pattern + the manifest item + the `BROWSER_STORAGE.md` row. Verified — shared
  vitest 19/19 green, four SPA builds green. No other issues (lazy Stripe, on-use Maps coverage,
  consentGate edges, `app/web/shared` boundary all verified clean).

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

### 2026-08-10 — Phase 2 removal applied then reverted; banner-removal decision corrected; research launched

- Action: Synced worktree to `origin/main` (was 8 behind / 3 ahead; clean merge). Applied the Phase-2
  removal as written (deleted consent machinery across `app/web` + the E2E suppression scaffold; rewired
  all four `main.tsx` + `Footer`; removed the `sidebar_state` cookie), verified green — four SPA builds ✓,
  `@concertable/web` vitest ✓ (empty-suite handled), both UI E2E projects compile ✓. Tommy then flagged
  that analytics/marketing tracking IS on the commercial roadmap, which invalidates the "decorative →
  remove" premise. **Reverted every consent-machinery deletion/edit** (`git checkout HEAD --` on all of
  them) and the incidental `routeTree.gen.ts` regens; **kept only** the dead `sidebar_state` cookie
  removal. Launched three background research agents (UK gig marketplaces; ticketing platforms + CMPs; UK
  PECR/ICO legal baseline + Consent-Mode + build-vs-buy) to ground the production-ready design.
- Evidence: `git status` after revert shows only `app/web/shared/src/components/ui/sidebar.tsx` modified;
  `CookieConsentBanner.tsx`/`consent.ts` back in `git ls-files`. Early findings: GigPig ships an
  analytics+marketing banner; Ticketmaster runs OneTrust (Necessary/Analytics/Advertising/Functional).
- Outcome: **Banner retained.** Phase 2 reduced to the single dead-cookie removal. The banner's real
  defect (toggles gate nothing) moves to Phase 3 "make it actually gate," design pending research.
- Follow-up: compile the three research reports into a repo doc + recommendation, then implement Phase 3
  (gating primitive + lazy Stripe/Maps + storage manifest).

### 2026-08-10 — research compiled; build-vs-buy decided (custom); Phase 3 started (lazy Stripe)

- Action: Compiled all three research passes into `plans/launch/CONSENT_RESEARCH.md` (committed with the
  dead-cookie removal). Tommy chose **custom real-gate** over a CMP. Started Phase 3 with the self-contained
  **lazy-Stripe** change and committed it (`de4cda25f`).
- Evidence: `CONSENT_RESEARCH.md`; four SPA builds green after the Stripe change; commits `2c31f44da`
  (dead cookie + research) and `de4cda25f` (lazy Stripe).
- Outcome: Stripe no longer loads on anonymous landing (the headline audit finding). Banner retained with
  Reject-all parity already correct. Remaining Phase 3: consent gating primitive, Maps off-boot, storage
  manifest, Phase 4 inventory doc.
- Follow-up: consent gating primitive next; Maps handling pending the on-use-vs-hard-gate UX decision (see
  Next Steps). Nothing pushed.

### 2026-08-10 — Phase 3 finished + Phase 4 done; engineering complete; pushed

- Action: Synced to `origin/main` (3 behind; clean merge). Built the **consent gating primitive**
  (`lib/consentGate.ts`: `registerConsentGated` + `registerConsentGatedScript`, 7 unit tests) — the
  integration point future GA4/pixels register against. Made **Maps load on-use**: removed the root
  `APIProvider` from all three `main.tsx` and added a scoped `providers/MapsProvider.tsx` mounted around
  `FindPage` and `GoogleMap` only. Added the **storage manifest** (`lib/storageManifest.ts`) + a
  **drift-guard test** that fails on any unclassified storage write in `app/web`. Wrote the Phase 4
  engineering inventory `app/web/shared/BROWSER_STORAGE.md`. Committed each as a discrete change and
  pushed the branch.
- Evidence: four SPA builds green after each change; shared vitest 19/19 green; commits `909af3791`
  (primitive), `a43e334cc` (on-use Maps), `86e4fba78` (manifest + drift guard), plus the Phase 4 doc +
  this ledger. Maps consumers confirmed as only `FindPage` (customer) + `GoogleMap` (detail pages, all
  apps); b2b find lists and landing/home render no Maps component, so they no longer contact Google.
- Outcome: **Engineering for Phases 1–4 complete.** Banner retained and now genuinely gating; both
  boot-time third parties (Stripe, Maps) load on use; every stored item classified + drift-guarded +
  documented. Remaining is delivery (review → PR → merge → platform-sync) + the legal-gated tail.
- Follow-up: `## Next Steps` — review, then PR/merge (full E2E tier) awaiting Tommy's go-ahead.
- Decision (no block): implemented Maps as **functional-necessary-on-use** (the plan/research-preferred
  durable shape, required in every branch); the additional `functional`-category consent gate is a
  solicitor call, documented as a superset that wraps the same mount points — not asked, not blocking.

### 2026-08-10 — code review; NAT1 fixed (drift guard blind to zustand persist)

- Action: Synced worktree to `origin/main` (was 9 behind / 12 ahead; clean merge — incoming commits
  all `api/**`, no `app/web` overlap). Ran a full two-layer code review over `origin/main..HEAD` (native
  `code-reviewer` subagent + architecture lenses); security layer skipped (no `_SECURITY_PATTERNS` path).
  Native layer surfaced **NAT1**: the storage drift-guard regex-matched only literal `setItem`/`[..]=`/
  `document.cookie=`/`indexedDB.open(`, so it was blind to zustand `persist()` — and `useTenantStore.ts`
  (`b2b/shared`, venue+artist) persists localStorage `concertable.active-tenant`, which was therefore
  both missing from `STORAGE_MANIFEST` and undetectable, making the "new storage can't ship
  unclassified" guarantee false. Verified against the real store, then fixed: added a `persist(` →
  localStorage `WRITE_PATTERN`, the `concertable.active-tenant` manifest item, and the `BROWSER_STORAGE.md`
  row (reworded the manifest note to avoid the scanner self-matching `persist(` in its own prose).
- Evidence: `reviews/Feature-launch_browser-storage-consent.md`; shared vitest **19/19 green** (drift
  guard now emits + matches the persist token); four SPA builds green (exit 0). Incidental
  `routeTree.gen.ts` line-ending regens reverted.
- Outcome: Review complete, one real defect fixed, all gates green. Layer-2 lenses found nothing else
  (lazy Stripe, on-use Maps coverage, consentGate edges, `app/web/shared` boundary all clean).
- Follow-up: `## Next Steps` 2 — PR + `/merge` at full E2E tier, awaiting Tommy's go-ahead. Nothing pushed.

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\launch_browser-storage-consent
Read @plans/launch/BROWSER_STORAGE_CONSENT_PLAN.md and @plans/launch/BROWSER_STORAGE_CONSENT_PROGRESS.md, then do what the ledger's `## Next Steps` says.
```
