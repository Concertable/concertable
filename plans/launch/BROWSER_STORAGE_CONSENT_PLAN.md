# Browser-storage audit + consent correction

> Next steps live in @plans/launch/BROWSER_STORAGE_CONSENT_PROGRESS.md → `## Next Steps`.
> This plan holds the design and outstanding phases; the ledger holds the operational truth,
> the full storage inventory, and the current next action.

## Why this plan exists

Concertable ships a generic cookie-consent banner (Accept all / Reject all / Manage, with
**Analytics** and **Marketing** toggles) on all four web SPAs. The launch item that spun this plan
off is **not** "polish the banner" — it is an evidence-led correction of the whole browser-storage
surface. The banner was added without an inventory of what the SPAs actually store, so three things
are unverified and one is already suspicious:

- The banner's Analytics/Marketing choices may gate **no real technology** (making the UI decorative,
  which under UK PECR/UK-GDPR is worse than no banner — it implies choices that do nothing).
- Necessary Auth/Stripe storage was never written down as an engineering-owned inventory that a
  solicitor can turn into a cookie/storage policy.
- A sidebar cookie is written on every toggle but may never be read (dead state).
- Whatever third-party scripts the SPAs load (Stripe, Maps) set their own storage that no one has
  catalogued or classified.

**The audit decides the banner's fate. This plan does not assume the banner is preserved, refactored,
or removed** — Phase 1 produces the evidence, and the decision gate at the end of Phase 1 chooses the
implementation. Everything after Phase 1 is written as "drive to the findings," and each removal is
justified by an inventory line, never by a hunch.

## The audit-first principle (non-negotiable ordering)

No storage is removed, no consent code is touched, and no test is deleted until the Phase 1 inventory
is complete and classified. "This looks unused" is a hypothesis; the audit is what promotes it to a
finding. The static code inventory is the backbone; the runtime capture across the three journeys is
what proves what is *actually* set in a real browser (third-party scripts set storage our code never
names). Both must be done before Phase 2 begins.

## Legal frame (for classification, not legal advice)

UK PECR reg. 6 requires consent **before** storing or accessing information on a user's device, with a
narrow **"strictly necessary"** exemption: storage that is *essential* to deliver a service the user
explicitly requested. UK-GDPR governs any personal data that storage then processes. Working rules the
audit classifies against:

- **Strictly necessary / exempt** — auth/session, security/fraud tied to an action the user asked for
  (e.g. Stripe fraud signals during checkout), load-balancing, and the record of the consent choice
  itself. No consent needed; still must be documented in the policy.
- **Functional** — remembers a user preference (theme, sidebar). Often exempt when it holds no PII and
  is set only in response to a user action, but must still be documented; the solicitor confirms.
- **Optional / consent-requiring** — analytics, marketing, personalisation, and any non-essential
  third-party embed that sets device storage before the user needs the feature. Consent must be
  obtained **before load**, and refusal must actually prevent the load.
- **Unused** — set but never read, or gating nothing. Remove; never classify or seek consent for it.

## Scope

- **In:** `app/web/**` (the four SPAs + `shared`), the consent/storage code, the browser-storage
  inventory doc, and the E2E/unit test machinery that exercises consent.
- **Out:** Privacy + T&Cs page *routes* (a separate Month-4 launch item), pricing transparency, and
  the solicitor's legal *drafting* itself. This plan produces the engineering inventory the solicitor
  drafts *from*; it does not write policy copy.

## Constraints (each phase honours all of these)

- **Shared-tree boundary.** Consent/storage code lives in `app/web/shared` and compiles into all four
  SPAs. No app-specific literals and **no `role === …` / `isVenueManager(…)` branching** in shared —
  variation is injected via props/slots (`app/web/shared/AGENTS.md`). If a behaviour must differ per
  app, the app owns it and passes it in.
- **All four builds green is the boundary gate**, every phase:
  ```
  npm -w @concertable/web-customer run build
  npm -w @concertable/web-venue    run build
  npm -w @concertable/web-artist   run build
  npm -w @concertable/web-business run build
  ```
  (`business` is `vite build` only, no `tsc -b`.) If a route file is added/renamed, regenerate that
  app's `routeTree.gen.ts` first.
- **Affected unit/integration tests** green each phase (the shared vitest suite covers
  `consent.test.ts`).
- **No `api/**` package boundary is crossed.** The only backend edits are E2E **test projects**
  (`api/Concertable.B2B/tests/**`, `api/Concertable.Shared/tests/**`) — no published `Concertable.*`
  package changes shape, so there is **no hard platform-sync gate**. A merge touching `api/**` still
  triggers a `chore/platform-sync-*` PR (MinVer bumps on every merge); because nothing a consumer
  compiles against changed, that sync PR is **non-breaking and auto-merges green** — follow it, but it
  is not a design constraint. (This is stated from evidence, not assumed — see the ledger.)
- **Merge-queue E2E tier** chosen on the final phase per `plans/AGENTS.md` "Merge-queue E2E tier."
  These changes alter a first-visit UI flow across multiple SPAs → default to **full E2E** (do not
  `skip-e2e`); do not duplicate the queue run locally.

## Phases

### Phase 1 — Evidence-led audit (produces everything the later phases depend on)

**Goal:** a complete, classified inventory of every item the SPAs store on the device, from both
static code and a real browser, across three journeys on every affected SPA.

1. **Static inventory (code sweep).** Enumerate every `localStorage` / `sessionStorage` /
   `document.cookie` / `indexedDB` write in `app/web/**`, and every third-party script the SPAs load
   that sets its own storage (Stripe, Google Maps, OIDC provider, any other). For each: name, API,
   where set, whether it is ever read back, which SPAs mount it. *(Substantially complete — recorded
   in the ledger's inventory tables; treat as the map to verify at runtime, not as the whole audit.)*
2. **Runtime capture (the proof).** With the stack running, drive each affected SPA through three
   journeys and dump **cookies + localStorage + sessionStorage + IndexedDB** after each:
   - **(a) Anonymous browsing** — landing/find pages, no login.
   - **(b) Authenticated session** — logged-in manager (venue/artist) and logged-in customer.
   - **(c) Stripe payer checkout** — customer ticket checkout, venue accept-checkout, artist
     apply-checkout (whichever set a payment/setup intent).

   Capture via the browser DevTools Application panel or a Playwright `context.storageState()` +
   `context.cookies()` dump per journey. Record the raw first-party **and** third-party items —
   third-party scripts set storage our code never names, so the static list is necessarily incomplete
   until this runs.
3. **Classify every item** against the legal frame above: name, owner (first-party vs third-party and
   which), purpose, duration, and one of **necessary / functional / optional-consent-requiring /
   unused**. Note specifically, for each optional item, *what technology it belongs to and whether
   anything loads that technology today.*
4. **Decision gate (chooses the implementation).** From the classified inventory, decide per finding:
   - storage/consent/test machinery that gates or backs **no active behaviour** → **remove** (Phase 2);
   - a genuine **non-exempt optional** technology that the SPAs actually load → **consent-before-load**
     or make-functional-on-use (Phase 3);
   - **necessary/functional** storage → **document** in the engineering inventory (Phase 4).

**Gate:** the inventory + classification table is complete for all three journeys on every affected
SPA, and each subsequent phase's work list is derived line-by-line from it. No code changes in this
phase; four builds remain green trivially (nothing changed).

### Phase 2 — Remove what the audit shows is unjustified

Only the items the Phase 1 table marks **unused** or **gates-nothing**. Each removal cites its
inventory line in the commit message. Candidates the static inventory already flags (to be confirmed
by the runtime capture, not pre-executed):

- **The write-only sidebar cookie** — `sidebar_state` is written in `app/web/shared/src/components/ui/
  sidebar.tsx` but never read (a client-only SPA has no SSR to consume it; `defaultOpen` is a literal).
  If the runtime capture confirms no read path, delete the write (and either drop persistence or move
  it to `localStorage` **only if** persisting sidebar state is actually wanted — decide from evidence,
  don't reflexively re-add it).
- **The generic consent machinery**, *if* the audit confirms it gates nothing: `consent.ts`,
  `consent.test.ts`, `ConsentProvider.tsx`, `CookieConsentBanner.tsx`, `CookiePreferencesDialog.tsx`,
  `ManageCookiesButton.tsx`, the wiring in all four `main.tsx`, and the `Footer.tsx` "Manage cookies"
  affordance — plus the E2E/test machinery that exists only to exercise it:
  `CookieConsent.feature`, `CookieConsentPage.cs`, `CookieConsentSteps.cs` (B2B UI), the shared
  `CookieConsentState.cs`, and the `establishDeniedCookieConsent` plumbing threaded through **both**
  the B2B and Customer `Browser.cs` (constructor flag, `InitializeAsync` parameter, and every call
  site in the UI fixtures/hooks). Removing the banner removes the reason those tests suppress it, so
  the suppression scaffold goes too.

**Do not remove `consent.ts` / the banner if Phase 3 keeps a consent mechanism** — in that case Phase 2
removes only the truly-unused pieces (sidebar cookie, dead categories) and Phase 3 reshapes the rest.
The audit's decision gate says which branch applies; the ledger records the decision with evidence.

**Gate:** four builds green; the shared vitest suite green (with `consent.test.ts` removed if the
banner is removed); the B2B + Customer UI E2E projects compile and their fixtures no longer reference
removed symbols.

### Phase 3 — Consent only where a real non-exempt optional technology exists, and it must gate loading

If — and only if — Phase 1 finds a genuine non-exempt optional technology the SPAs load, design a
consent mechanism that **actually gates that technology's loading** (consent-before-load), not a
decorative banner. The static inventory's leading candidate is **Google Maps JS API**, loaded
unconditionally at app boot via `MapsProvider` in `main.tsx` on customer/venue/artist (used by
`SearchBar` places autocomplete and the `GoogleMap` component). Two durable options, chosen from what
the runtime capture shows Maps actually stores and with solicitor input on Maps' status:

- **Make it functional-on-use** — load Maps lazily only on the routes/components that need it (find
  pages, address autocomplete) instead of at boot. If it only loads when the user invokes a
  map/search feature they asked for, it moves toward the functional/necessary-on-use side and the
  consent surface shrinks or disappears. This is the preferred long-term shape regardless.
- **Gate it behind consent** — if the solicitor deems Maps non-exempt even on-use, wire a
  consent-before-load primitive: the Maps script is not requested until consent for that specific
  purpose is granted, and refusal degrades the feature gracefully (manual address entry, no map).

**Durable governance design (the real deliverable of this phase):** a single source-of-truth storage
manifest in `app/web/shared` that lists every storage item with owner/purpose/duration/classification,
and a test that fails when code introduces a storage write not present in the manifest — so new storage
must be classified to compile-green, and the engineering inventory (Phase 4) can be generated from it
rather than hand-maintained and drifting. If a consent mechanism is kept, it reads the manifest to know
what it gates. This is the "where new storage gets classified, how consent gates loading if any is
needed" answer the roadmap asked for — not a one-off patch.

**Gate:** four builds green; the consent-before-load behaviour (if any) covered by unit tests and, if
it changes a first-visit flow, by an E2E scenario that asserts the gated technology does **not** load
before consent and **does** after.

### Phase 4 — Engineering-owned storage inventory doc (feeds the solicitor policy)

Write the necessary/functional storage inventory as an engineering doc (generated from or checked
against the Phase 3 manifest) at `app/web/shared/BROWSER_STORAGE.md` (or the lowest node that fully
contains the concern per the doc-locality rule). It lists, for every retained item: name, owner,
purpose, duration, lawful basis/classification, and which SPAs set it. This is the artifact the
solicitor turns into the public cookie/storage policy.

**Dependency (documented, not blocking):** the solicitor-drafted cookie/storage **policy copy** is a
Month-4 legal input. Phases 1–4 (audit, removal, correction, engineering inventory) proceed **now** and
do not wait. **Only** the final step — wiring the solicitor's finished policy wording into the
`/cookies` page and reconciling the inventory doc with it — is gated on that input, and that page route
is itself the separate Month-4 item. So this plan reaches "engineering-complete" without the solicitor;
it records the one downstream wording-wire as the sole legal-gated tail.

## Definition of done

- Phase 1 inventory + classification complete for all three journeys on every affected SPA, recorded
  in the ledger.
- Every storage item is one of: removed (unused), documented (necessary/functional), or
  consent-gated-before-load (non-exempt optional). No decorative consent UI remains — any consent
  surface that ships gates a real technology.
- The durable classification manifest + drift-guard test is in place, so future storage is classified
  by construction.
- `app/web/shared/BROWSER_STORAGE.md` engineering inventory exists and matches the manifest.
- All four web builds green; affected unit/integration green; merge-queue E2E tier selected and green.
- Roadmap `🟡 Browser-storage audit + consent correction` line and the §7 "Compliance UI/UX" checklist
  item ("Browser storage inventory complete; unnecessary storage removed; necessary Auth/Stripe storage
  documented; any retained consent UI gates real optional technology") ticked **in the same commit as
  the shipping work** — never delete the roadmap.
- The single solicitor-wording wire is the only item left gated on the Month-4 legal input, recorded
  as such in the ledger.

## References

- `app/web/AGENTS.md`, `app/web/shared/AGENTS.md` — shared-tree boundary + four-builds gate.
- `plans/AGENTS.md`, `plans/agents/PLAN.md` — plan/ledger lifecycle, phases-each-green, E2E tier.
- `plans/launch/LAUNCH_CHECKLIST.md` Phase 2 — the three reopened data-protection items this plan closes.
