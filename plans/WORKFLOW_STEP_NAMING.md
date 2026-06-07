# Investigation: Concert workflow step naming & reuse

## Prompt (run this in a fresh session, AFTER the FSM seed-factory slice 2 is finished and the tree is green)

Investigate and propose a renaming + consolidation of the Concert module's workflow **step** implementations in `api/Concertable.B2B/Modules/Concert/`. The governing principle (owner's, non-negotiable):

> **A step should be named after what it does, and be reusable across workflows. Name it after a contract type ONLY when its behaviour is genuinely unique to that contract AND cannot be made polymorphic. A future contract type should be able to reuse existing steps by tapping them into its chain — not force a copy-paste.**

The good model already in the tree: `SimpleApplyStep` / `PaidApplyStep` are named by *capability* (simple vs paid), not by contract, and are shared across workflows. Everything else should be held to that bar.

### Context (established in the FSM refactor — read `plans/CONTRACT_LIFECYCLE_FSM.md` first)

The workflow is: per-`ContractType` fluent chain in `ConcertWorkflowBuilder` registers steps; steps are pure effects fired by the lifecycle transitioner; `CreateConcertDraftStep` (`IBookStep`) is already the consolidated "on Booked" effect shared by all four contracts (good precedent — follow it).

Step implementations live in `…Concert.Infrastructure/Services/Workflow/Steps/`; their interfaces in `…Concert.Application/Workflow/Steps/`. Current set:
- **Apply:** `SimpleApplyStep`, `PaidApplyStep` (already generic — the model)
- **Accept:** `FlatFeeAcceptStep`, `VenueHireAcceptStep` (both `ISimpleAcceptStep`), `PaidAcceptStep` (`IPaidAcceptStep`)
- **Checkout:** `FlatFeeAcceptCheckoutStep`, `DoorSplitAcceptCheckoutStep`, `VersusAcceptCheckoutStep` (all `IAcceptCheckoutStep`), `VenueHireApplyCheckoutStep` (`IApplyCheckoutStep`)
- **Book:** `CreateConcertDraftStep` (already consolidated)
- **Finish:** `FlatFeeFinishStep`, `VenueHireFinishStep`, `DoorSplitFinishStep`, `VersusFinishStep`

### Known duplications already spotted (verify, then fix)

1. **`DoorSplitAcceptCheckoutStep` ≡ `VersusAcceptCheckoutStep`** — byte-identical except the one returned payment-breakdown object: `new DoorSharePayment(contract.ArtistDoorPercent)` vs `new GuaranteedDoorPayment(contract.Guarantee, contract.ArtistDoorPercent)`. Same deps, same `type=Verify` metadata, same `CreateVerifySessionAsync`, same `Settlement` label. → should be ONE step that creates the verify session; the per-contract payment breakdown is the only thing to make polymorphic.

2. **`FlatFeeFinishStep` ≡ `VenueHireFinishStep`** (confirm against current files) — both appeared to be identical escrow-release bodies (`bookingRepository.GetIdByConcertIdAsync` → `escrowClient.ReleaseByBookingIdAsync`). → likely one generic escrow-release finish step.

3. **`FlatFeeAcceptCheckoutStep` is NOT a verify checkout** — it uses `CreateHoldSessionAsync` (escrow hold), `type=applicationAccept`, `Charge` label, `FlatPayment`. Genuinely a different (escrow/hold) checkout. Keep separate, but rename honestly (see naming question below).

4. **`DoorSplitFinishStep` vs `VersusFinishStep`** — genuinely different payout math (DoorSplit: `revenue × pct`; Versus: `guarantee + revenue × pct`), but both then call the same `managerPaymentClient.PayAsync(...)`. Evaluate whether the math is a polymorphic contract concern leaving one generic "settle payout" finish step, or whether they stay distinct.

### Naming questions to answer

1. **Name checkout steps by the payment they create, not the contract**: e.g. `VerifyCheckoutStep` (DoorSplit+Versus, merged), `EscrowCheckoutStep`/`HoldCheckoutStep` (FlatFee), `SetupCheckoutStep` (VenueHire apply). Confirm each maps 1:1 to a Payment-service session method (`CreateVerifySessionAsync` / `CreateHoldSessionAsync` / setup-session) so the name reflects the real service call.

2. **Is the `Apply`/`Accept` lifecycle prefix on checkout steps (and the `IApplyCheckoutStep` vs `IAcceptCheckoutStep` split) carrying its weight?** Today exactly one checkout happens per workflow, and the prefix encodes *when* it fires. Investigate: does the architecture actually support (or want to support) multiple checkouts per workflow? If not, the prefix is noise and the moment is already implied by where the chain taps it in. If multiple checkouts are plausibly wanted later, the split earns its keep. Recommend keep-or-drop with reasoning, not a guess.

3. **The payment-breakdown polymorphism seam** — `DoorSharePayment` / `GuaranteedDoorPayment` / `FlatPayment` (in `…Application/Responses/`). For a single `VerifyCheckoutStep` to serve DoorSplit+Versus without a `switch`, the contract must produce its own breakdown. Find the cleanest seam: does the contract type already expose this, is there a mapper, or should the breakdown be derived polymorphically from `IContractAccessor.Contract`? This same seam likely resolves the DoorSplit/Versus finish-step math.

### Deliverable

1. A table of every step impl: **genuinely contract-specific** (keep, justify why it can't be polymorphic) vs **generic-but-misnamed** (rename + consolidate).
2. Proposed final names + which interfaces survive / merge / get the lifecycle prefix dropped.
3. The polymorphism design for the per-contract bits (payment breakdown, payout math) so the consolidated steps stay `switch`-free.
4. Blast radius: the steps are out of the FSM refactor's tested core, so this is a rename/merge pass — list every `ConcertWorkflowBuilder` chain edit, DI registration, and workflow-class field change. No behaviour change; the integration suite (Concert 56) + `e2e-ui-regress` must stay green.

### Constraints

- Behaviour-preserving rename/consolidation only — no lifecycle/machine changes (the FSM is settled).
- Follow `SimpleApplyStep`/`CreateConcertDraftStep` as the naming precedent (capability/action, not contract).
- C# conventions per `CODE_CONVENTIONS.md` + repo memory (explicit ctors, no underscores, `internal` + IVT, no narration comments).
- Prerequisite: FSM seed-factory slice 2 finished and full gate green BEFORE starting this — do not stack it on a half-done tree.
