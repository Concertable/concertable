# Service documentation & guidance locality — progress

- **Plan:** `plans/platform/SERVICE_DOC_LOCALITY_PLAN.md`
- **Worktree:** none — docs-path, no dedicated worktree needed (docs exempt from branch hygiene)
- **Branch:** not yet started (light `Feature/platform_service-doc-locality`, or the current branch)
- **PR:** none
- **Gates:** docs-path — no build/unit/integration/E2E; merges via `/merge-docs`

## Current state

All three phases complete in the working tree (uncommitted). Scope is docs-first (ownership rule + per-service
`AGENTS.md`/`ARCHITECTURE.md` gaps). The `plans/` relocation is deliberately out of scope, deferred until
the polyrepo end-state layout is decided. Next: commit + `/merge-docs` (no-E2E docs path); the plan's local
phases are done.

## Completed work

- **Phase 1 (ownership rule) — working tree, uncommitted.** Added the "lowest fully-containing node"
  doc-locality rule to root `AGENTS.md` (under "Per-area guidance") and a cross-reference sentence in the
  microservices section of `api/AGENTS.md`, naming `Concertable.Payment` as the thin-file template.
- **Phase 2 (service-root `AGENTS.md` gaps) — working tree, uncommitted.** Added thin `CLAUDE.md` (`@AGENTS.md`)
  + `AGENTS.md` pairs for **B2B** (identity-only/active-tenant authority; agent-VAT + VenueHire footgun;
  Deal≠Contract), **Customer** (marketplace-agent posture; artist/venue projected twice; webhook-minted
  tickets), and **Auth** (Razor-Pages UI; in-code Duende config; two migration contexts + grant store in
  `B2BDb`). **Search + Messaging `AGENTS.md` skipped** (lazy creation — nothing beyond upward guidance).
  Each new file is pointer-first and restates nothing upward.
- **Phase 3 (service `ARCHITECTURE.md`) — working tree, uncommitted.** Created `api/Concertable.Payment/ARCHITECTURE.md`
  (agnostic adapter posture; double-entry ledger + posting recipes; escrow two-phase hold/reserve-first refund;
  commission/VAT engine; keyed `ITransactionHandler` dispatch by metadata `type`; Stripe real-vs-`Fake` seams;
  webhook pipeline + two-layer idempotency; Connect-Express event-provisioned payout accounts; gRPC surface +
  `owner` boundary; events; auth; tech stack) and `api/Concertable.Search/ARCHITECTURE.md` (thin B2B/Customer
  template — read/write host split; separate read-model vs rating-projection tables LEFT-joined at read time;
  six-event→handler→table map; `HeaderType`-keyed read API + NTS geo; B2B-contracts-only carve). Extended
  `api/Concertable.Payment/AGENTS.md` to the thin service-root pattern (inheritance header + `ARCHITECTURE.md`
  pointer; agnostic-`type` rule; never-seed ledger/escrow/payout; E2E/dev-never-real-Stripe; `long` minor-units).
  **Messaging skipped** (shared infra library, not a data/adapter service). All grounded in two deep code sweeps,
  not the plan's remembered spec — three plan claims corrected (see Decisions).

## Verification and review state

Coherence-read gate (docs-path, no build): each new file is service-specific, inherits upward, restates nothing.
Grounded against verified code sweeps of both services.

## Decisions, discoveries, blockers, and deviations

- **Docs-first scope.** Only the ownership rule + doc gaps; relocating the cross-cutting `plans/` tree is
  deferred because it's entangled with the undecided end-state layout (`services/<x>/{api,web,mobile}` vs.
  mirror assembler).
- **Lazy creation.** Create a service-root `AGENTS.md`/`ARCHITECTURE.md` only where genuine
  service-specific content exists; skipping a service with nothing beyond upward guidance is a valid
  outcome, recorded, not a gap to pad.
- **Gap map (verified 2026-08-05):** missing service-root `AGENTS.md` — B2B, Customer, Auth, Search,
  Messaging; missing `ARCHITECTURE.md` — Payment, Search, Messaging.
- **Phase 2/3 verdict (from the four sweeps).** `AGENTS.md`: CREATE B2B, Customer, Auth; SKIP Search +
  Messaging (rules already upward, no local footgun). `ARCHITECTURE.md`: CREATE Payment + Search; SKIP
  Messaging (a shared library not a service — pattern is upward, internals self-documenting; a short README
  would serve better, out of scope).
- **By-product findings (out of scope — surfaced for decision, NOT acted on):**
  - Customer `Review/.../ConcertReviewsController.cs` (incl. the review-submit POST) appears to lack the
    `[Customer]` auth attribute other Customer controllers carry — possible authz gap; verify.
  - Auth `ARCHITECTURE.md:54-55` is stale — cites Payment's removed `ManagerRegisteredHandler` (now
    `PayoutOwnerRegisteredHandler` on `PayoutOwnerRegisteredEvent`).
  - Duende persisted-grant store physically lives in `B2BDb` — a per-service-DB boundary smell; candidate
    `TECH_DEBT` line.
  - `Modules/Deal/LEGAL_REQUIREMENTS.md` is a service-wide B2B compliance doc mis-parked in the Deal module
    — candidate to lift to `api/Concertable.B2B/` root (would update the `LAUNCH_ROADMAP` link). Customer's
    `LEGAL_REQUIREMENTS.md` is correctly at root but orphaned (unreferenced) — link it from `ARCHITECTURE.md`.
  - **Payment `ConcertChangedEvent` topology subscription is vestigial** — `PaymentTopology.cs` declares an ASB
    subscription for `ConcertChangedEvent`, but no host `Program.cs` calls `SubscribeTo<ConcertChangedEvent>` and
    there is no handler. Dead wiring; candidate `TECH_DEBT` line (deliberately kept out of `ARCHITECTURE.md`, which
    documents real architecture only).
- **Phase 3 plan-claim corrections (from the code sweeps).**
  - Search serves **no entity-details** endpoint (only browse/search + autocomplete + geo); details pages are the
    B2B/Customer frozen wire contract. Plan's "details reads" dropped.
  - Search's rating events are **B2B-owned contracts** (`*RatingUpdatedEvent`), not Customer's — Customer is the
    *origin* (review), B2B recomputes and re-publishes; Search references only `Concertable.B2B.*.Contracts`, zero
    Customer dependency. Plan's "B2B's and Customer's contracts" corrected.
  - `Money` is a **major-unit decimal** value object, not minor-units; the ledger / `*Minor` fields / calculator
    use `long` minor units and `Money` converts at the edges. Payment `AGENTS.md`/`ARCHITECTURE.md` state this
    precisely rather than the plan's looser "`Money` minor-units". Payment's keyed dispatch is the
    keyed-service-locator variant, not the canonical `FrozenDictionary` facade — documented as such.

## Next Steps

**All three local phases are done (working tree).** Remaining lifecycle: commit the doc work and take it
through `/merge-docs` (no-E2E docs path). On merge, delete the plan + this ledger in the close-out change
(git history is the archive). The by-product findings under Decisions are separate follow-ups, not part of
this plan.

## Event log

### 2026-08-05 — Plan and ledger created
- Action: Wrote the plan and this ledger at docs-first scope; recorded the gap map, the governing
  ownership rule, and the deferral of the `plans/` relocation.
- Outcome: §4 of the polyrepo epic now has an owning plan; ready to start Phase 1.
- Follow-up: Execute Phase 1 (ownership rule) on greenlight.

### 2026-08-05 — Phase 1 complete (working tree)
- Action: Added the "lowest fully-containing node" doc-locality rule to root `AGENTS.md` ("Per-area
  guidance") and a cross-reference sentence to the microservices section of `api/AGENTS.md`, naming
  `Concertable.Payment` as the thin-file template.
- Outcome: the governing rule is single-sourced; Phases 2–3 apply it. Uncommitted, will ride the next commit.
- Follow-up: Phase 2 — service-root `AGENTS.md` gaps.

### 2026-08-05 — Phase 2 investigation dispatched
- Action: Launched four parallel per-service sweeps (B2B; Customer; Auth+Payment; Search+Messaging) to
  enumerate genuinely service-specific guidance (vs. what's already upward) and assess `ARCHITECTURE.md`
  need for Payment/Search/Messaging. Investigation only; no files written.
- Outcome: awaiting results to produce the create/skip verdict per service.
- Follow-up: synthesize findings, write warranted thin `AGENTS.md` files, skip/record the rest.

### 2026-08-05 — Phase 2 complete (working tree)
- Action: All four sweeps returned. Applied the verdict — created thin `CLAUDE.md` (`@AGENTS.md`) +
  `AGENTS.md` pairs for B2B, Customer, Auth; skipped Search + Messaging `AGENTS.md` (lazy creation).
  Recorded the four out-of-scope by-product findings under Decisions.
- Outcome: 3 service-root `AGENTS.md` created; 6 files written; Phase 2 done. Uncommitted.
- Follow-up: Phase 3 — Payment + Search `ARCHITECTURE.md` and Payment `AGENTS.md` additions.

### 2026-08-05 — Phase 3 complete (working tree)
- Action: Dispatched two deep code sweeps (Payment; Search), then wrote `api/Concertable.Payment/ARCHITECTURE.md`,
  `api/Concertable.Search/ARCHITECTURE.md`, and extended `api/Concertable.Payment/AGENTS.md` to the thin
  service-root pattern. Messaging skipped (shared library). Corrected three plan claims against verified code
  (Search has no details endpoint; Search rating events are B2B-owned contracts; `Money` is major-unit, ledger
  math is `long` minor-units). Surfaced the vestigial Payment `ConcertChangedEvent` subscription as a by-product.
- Outcome: 3 files written; all local phases done. Search `TECH_DEBT.md`'s link to `./ARCHITECTURE.md` no longer dangles.
- Follow-up: commit + `/merge-docs`, then close out (delete plan + ledger) on merge.
