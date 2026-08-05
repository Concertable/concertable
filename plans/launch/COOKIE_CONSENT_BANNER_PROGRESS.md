# Cookie consent banner progress

- Plan: `plans/launch/COOKIE_CONSENT_BANNER_PLAN.md`
- Worktree: not yet created — stand up with `/worktree create Feature/launch_cookie-consent-banner`
- Branch: `Feature/launch_cookie-consent-banner` (created off `origin/main`, plan + ledger committed here)
- PR: not opened
- Dependency/package gates: none pre-merge. Post-merge: no `api/**` service change → no MinVer/platform-sync expected (a sync PR only if E2E test files under `api/**/tests` trip the publish path-filter; would be non-breaking). Text-only dependency (solicitor copy, Month 4) and the `/privacy` + `/cookies` routes are **out of scope** — placeholders here, not gates.
- Last reconciled: 2026-08-05 — branch created off `origin/main` (0 behind); no open **red** `chore/platform-sync-*` (only #373, green/pending); plan + this ledger authored. No implementation yet.

## Current state

**Plan authored; no code written.** Branch `Feature/launch_cookie-consent-banner` carries only the
plan + this ledger. The design is fixed: a router-free, universal consent banner in `app/web/shared`
(`lib/consent.ts` + `providers/ConsentProvider.tsx` + three components), mounted on all four SPAs
(customer, venue, artist, business), client-side persisted consent (localStorage, versioned), three
categories (necessary always-on / analytics off / marketing off), placeholder policy anchors, unit
tests for the logic + a UI E2E for accept/reject/manage. Implementation runs in a worktree, not the
main checkout.

## Next Steps

Stand up the worktree and implement the three phases (all one PR — no gate splits them):

1. **`/worktree create Feature/launch_cookie-consent-banner`** and work there (not the main checkout).
2. **Phase 1 — consent core + provider.** `app/web/shared/src/lib/consent.ts` (types, `CONSENT_STORAGE_KEY`, `CONSENT_VERSION`, `readConsent`/`writeConsent`/`hasConsent`/`onConsentChange`) + `providers/ConsentProvider.tsx` + `useConsent` (mirror `ThemeProvider.tsx`). Add a vitest config to `@concertable/web` (copy `app/web/b2b/venue/vitest.config.ts`, node env) + a `test` script + `lib/consent.test.ts`. Gate: four builds + `npm -w @concertable/web run test` green.
3. **Phase 2 — UI.** `CookieConsentBanner`, `CookiePreferencesDialog`, `ManageCookiesButton` (router-free; equal-weight accept/reject; `ui/dialog`+`ui/checkbox`+`ui/button`). `Footer.tsx` gains the manage-cookies trigger + placeholder `<a href="/cookies">`/`/privacy` anchors. Gate: four builds.
4. **Phase 3 — mount + E2E.** Wrap all four `main.tsx` in `<ConsentProvider>` + render `<CookieConsentBanner/>`; add the business inline-footer trigger. Author `CookieConsent.feature` (+ steps + page object) in `Concertable.B2B.E2ETests.Ui`, reconcile `E2E_BASELINE.md`; scenarios: reject-all (persists, dismisses, no non-essential cookie, survives reload, re-open via Manage) and accept-all. Gate: four builds; let the merge queue run **full** E2E (do not skip, do not duplicate locally).
5. **Commit** each phase when green. **Tick the roadmap** (§5 scaffolding row + §7 checkbox → ✅, correct "3 SPAs" → four apps) **in the same commit** as the feature. Then `gh pr create` → `/merge` (full E2E) → own any sync PR to green → doc-only close-out (`git rm` plan + ledger, riding the next change).

Read the plan for the file-by-file design and the per-decision rationale; do not restate it here.

## Completed work

- 2026-08-05 — `Feature/launch_cookie-consent-banner` created off `origin/main`; `COOKIE_CONSENT_BANNER_PLAN.md` + this ledger authored. No feature code yet.

## Verification

None yet — no code written. Gates defined in plan §6: four web builds green + `@concertable/web`
vitest suite (local) + full merge-queue UI E2E.

## Reviews

None yet.

## Decisions, discoveries, blockers, and deviations

- **D1 — client-side persisted consent is the PECR record; no server-side proof for v1.** Consent is captured pre-auth (no identity to key a server record to); ICO treats a durable client record (categories + timestamp + version) as demonstrating cookie consent; a server ledger would add identifying processing for no gain. ICO-aligned standard, not a shortcut. Server-side record = deferred marketplace-era enhancement.
- **D2 — localStorage (key `cookie-consent`), not a cookie** — mirrors the `theme` precedent, never transmitted, itself strictly-necessary.
- **D3 — router-free by construction** — forced by the business app (no TanStack Router / no provider stack); also lets a placeholder policy anchor (`<a href="/cookies">`) outlive a not-yet-existent route without breaking the typed-route `tsc` check.
- **D4 — three categories:** necessary (always on, no toggle) / analytics (off) / marketing (off). No non-essential cookies exist today — this ships the gate, not any gated script.
- **D5 — versioned consent** — `CONSENT_VERSION` bump re-prompts; the mechanism for the Month-4 copy/category changes to force fresh consent without a migration.
- **D6 — test split follows CI reality:** web vitest is **not** a merge-queue gate (confirmed: CI jobs are build/carve/unit(.NET)/integration(.NET)/e2e-api/e2e-ui; the SPAs are only built via `build:web-packages` and exercised through UI E2E). So logic → vitest unit (local, mirrors `b2b` `.test.ts`), rendered flow → UI E2E (authoritative gate). No jsdom component harness — CI wouldn't run it. Web-vitest-in-CI is pre-existing debt for `app/web/TECH_DEBT.md`, not this PR.
- **D7 — mounted on all four SPAs** — customer's inclusion is ~free future-proofing (shared compiles into it anyway; it already sets OIDC cookies).
- **Discovery — "3 SPAs" is stale.** The roadmap/checklist "customer/venue/artist" predates the **web-business** gateway carve-out; the real set is four apps (`customer`, `b2b/venue`, `b2b/artist`, `b2b/business`). The banner is universal `shared` UI compiled into all four; tick the roadmap in four-app language.
- **Discovery — no non-essential cookies today.** Only cookies/storage in use: OIDC auth (strictly necessary), the `sidebar:state` UI cookie (`ui/sidebar.tsx`), `theme` in localStorage — all functional/essential. The banner is forward-looking scaffolding.

## Event log

### 2026-08-05 — plan spun off the launch roadmap (Swim-lane C cookie-banner scaffolding)

- Action: read the launch roadmap (§5 Swim-lane C row + §7 checkbox + §3 Month-2 timing + R2), `LAUNCH_CHECKLIST.md` (Phase 2 data-protection framing), the plan/ledger convention (`plans/AGENTS.md`, `plans/agents/PLAN.md` + `ROADMAP.md`), the web topology (`app/web/AGENTS.md` + `shared/AGENTS.md`), and the `MUSIC_LICENCE_ATTESTATION` plan/ledger as the shape/size/naming precedent. Mapped the real web surface: four apps (`customer`, `b2b/venue`, `b2b/artist`, `b2b/business`); the business app is static/router-free; `ThemeProvider` is the localStorage-persisted-provider precedent to mirror; the design system has `dialog`/`checkbox`/`button`/`label`/`separator` (no `switch`); no analytics/marketing tooling and no non-essential cookies exist; web vitest lives only in `b2b/shared`+`b2b/venue` and is **not** a CI gate; UI E2E is Reqnroll+Playwright `.feature` files in `Concertable.B2B.E2ETests.Ui`. Created `Feature/launch_cookie-consent-banner` off `origin/main`; wrote the plan + this ledger.
- Evidence: `git rev-list --count HEAD..origin/main` = 0; open `chore/platform-sync-*` = #373 (green/pending, not red); no existing cookie plan in `plans/launch/`; four app package.json at `app/web/customer`, `app/web/b2b/{venue,artist,business}`; CI `test.yml` jobs = changes/build/carve-*/unit-tests/integration-tests/e2e-api-tests/e2e-ui-tests/e2e-ui-quarantine (no web-vitest job).
- Outcome: design fixed; ready to stand up the worktree and implement Phase 1.
- Follow-up: implement per `## Next Steps`.

## Resume prompt

```
/worktree create Feature/launch_cookie-consent-banner
Read @plans/launch/COOKIE_CONSENT_BANNER_PLAN.md and @plans/launch/COOKIE_CONSENT_BANNER_PROGRESS.md, then do what the ledger's `## Next Steps` says.
```
