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
- Last reconciled: 2026-08-10 — static code sweep of `app/web/**` + E2E test projects; branch created
  off `origin/main` (HEAD `c7968828e`).

## Current state

Worktree created off `origin/main`. Plan + this ledger authored. **Phase 1 static inventory is
substantially complete** (recorded below) and already yields a decisive picture; the **runtime capture
(Phase 1 step 2) is the remaining Phase 1 work** and must run before any Phase 2 removal. No `app/web`
or test code has been changed yet — the four SPA builds are at their `origin/main` baseline.

Branch-time gate at creation: 0 open red `chore/platform-sync-*` PRs; `origin/main` carried the
reopened roadmap line and the merged `Docs/launch_cookie-storage-audit` PR #469.

## Next Steps

Complete Phase 1 by running the runtime capture, then classify and open the decision gate:

1. Boot the stack (Aspire AppHost + the four SPAs) per the repo's run workflow. If any browser
   automation is used, honour the Docker-health pre-flight (`./docker-health.ps1`) only if a fixture
   stack is involved; a plain `npm run dev` of the SPAs plus a real Chrome session needs no Docker.
2. For each affected SPA (customer, venue, artist; business is static/no-auth), drive the three
   journeys and dump **cookies + localStorage + sessionStorage + IndexedDB** after each:
   (a) anonymous landing/find; (b) authenticated session (manager + customer); (c) Stripe payer
   checkout (customer ticket checkout, venue accept-checkout, artist apply-checkout). Use the DevTools
   Application panel or a Playwright `context.cookies()` + `context.storageState()` dump per journey.
3. Confirm/extend the static inventory tables below with the **actual** items observed — especially
   third-party storage our code never names (Stripe `__stripe_mid`/`__stripe_sid`; anything Google
   Maps sets at boot; the exact `oidc.*` localStorage keys).
4. Classify every observed item (necessary / functional / optional-consent-requiring / unused) and
   record the **decision gate** outcome in this ledger: which items go to Phase 2 (remove), Phase 3
   (consent-before-load / functional-on-use), Phase 4 (document).
5. Only then start Phase 2, each removal citing its inventory line.

## Completed work

- **Worktree + branch** `Feature/launch_browser-storage-consent` off `origin/main` (this session).
- **Plan + ledger authored** — `plans/launch/BROWSER_STORAGE_CONSENT_PLAN.md` + this file (committed
  with this ledger update).
- **Phase 1 static inventory** — complete code sweep of `app/web/**` and the E2E consent machinery
  (evidence below).

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

## Verification

None yet — no `app/web` or test code changed. Four SPA builds are at the `origin/main` baseline
(`c7968828e`). Verification begins with Phase 2's first change.

## Reviews

None yet.

## Decisions, discoveries, blockers, and deviations

- **Discovery — banner gates nothing.** The strongest audit signal: consent decision has no consumer
  and no analytics/marketing tech exists. Removal of the generic machinery is the leading Phase-2
  branch, pending runtime confirmation. (Recorded, not yet executed — audit-first ordering.)
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

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\launch_browser-storage-consent
Read @plans/launch/BROWSER_STORAGE_CONSENT_PLAN.md and @plans/launch/BROWSER_STORAGE_CONSENT_PROGRESS.md, then do what the ledger's `## Next Steps` says.
```
