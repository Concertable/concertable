# Organiser direct offers

> **Next steps live in @plans/launch/BOOKING_ENTRY_DIRECT_OFFERS_PROGRESS.md → `## Next Steps`.**

Baseline: `origin/main` at `516f4cc25`, the merge of PR #633. Every claim about current behaviour below
was read on that tree.

## 1. The problem

An opportunity is created by a venue and answered by an artist. `ApplicationWorkflow.ApplyAsync`
resolves *the current artist*, records the artist's signature and creates the row;
`AcceptCoreAsync` records *the venue's* signature and raises the accepted snapshot. There is exactly one
direction of travel, and it is hardcoded on both ends.

We also want the organiser to pick an artist and send an offer that the artist accepts, declines or
counters.

## 2. The decision — `Applied` and `Offered` are one thing seen from two sides

A pending application and a pending offer are the same fact: **a proposal is standing, and one named
party owes an answer.** They differ only in whose turn it is. Model exactly that, and the second
direction costs one state and one trigger instead of a parallel pipeline.

```text
                    Counter
             ┌──────────────────┐
             ▼                  │
   Applied ──┴──────────────► Offered          Applied  = the organiser owes an answer
      │        Counter          │              Offered  = the artist owes an answer
      ├── Accept ──► Accepted ◄─┤
      ├── Reject ──► Rejected ◄─┤
      └── Withdraw ► Withdrawn ◄┘
```

Both existing routes keep their current meaning: a public application lands in `Applied`, and today's
venue Accept/Reject and artist Withdraw are unchanged. An offer lands in `Offered`, and the same three
verbs apply with the parties swapped.

This makes the three verbs symmetric and kills the actor-specific branching:

| Verb | Who may fire it |
|---|---|
| `Accept` | the party whose turn it is — it signs the standing proposal and ends the negotiation |
| `Reject` | the party whose turn it is — declines the standing proposal |
| `Withdraw` | the party that authored the standing proposal — retracts it |
| `Counter` | the party whose turn it is — replaces the terms and hands the turn back |

`Cancel` from `Accepted` is untouched.

**Authority is therefore never "artist or venue".** It is derived from the row's own
`VenueTenantId`/`ArtistTenantId` pair plus the current state. That is the change that makes a later
promoter organiser a matter of *who resolves to the organiser side*, not a rewrite of every check.

## 3. What Opportunity owns, and where the offer belongs

**Opportunity owns the slot and its admission.** Venue, period, genres, the deal that states the
advertised terms, its open/filled state, and one new fact: **how candidacies may be created**.

```csharp
public enum OpportunityAdmission
{
    PublicApplications,
    TargetedOffers,
    Both
}
```

Admission is not visibility and not authority. It answers only "may a candidacy be created by this
route", and it is a column, not a strategy.

**The offer is an Application row.** It is a candidacy with a named counterparty, a consent, and a
terminal decision — precisely what the Application stage owns after PR #633. Putting it on Opportunity
would need a second path to acceptance, exclusivity, the accepted snapshot and booking creation.

**The offer never creates the counterparty's consent.** An offer records the organiser's signature and
nothing else. No booking, no commitment, no charge exists until the artist accepts.

## 4. No `OpportunityWorkflow` — the dependency direction decides it

`ApplicationWorkflow` already depends on `IOpportunityModule`. Opportunity does not depend on
Application; it only reacts to the accepted integration event. A workflow inside Opportunity that
created the initial proposal would invert that edge and close a cycle between two modules that PR #633
deliberately separated.

It would also not buy atomicity. Opportunity, Application and Deal each own their own `DbContext`, so no
single class can make "create the slot and the offer" one transaction. `OpportunityService.CreateAsync`
already creates a deal and an opportunity across that boundary today; the shape is known and the fix for
it is a claim, not a coordinator.

**So the offer is one operation owned by the Application module**, which asks Opportunity for the slot
when the request does not name one. Durability comes from `OperationClaim` — the abstraction whose whole
purpose is that a retried request resumes the same operation instead of starting a second one — taken on
both rows under one caller-supplied operation id:

- the offer request carries an operation id;
- Opportunity returns the slot already claimed by that id, or creates one and claims it;
- the Application row claims the same id as it is created.

A retry then reads two rows and creates nothing. A coordinator class gives no such guarantee.

`OpportunityWorkflow` becomes justified when Opportunity itself grows state that must roll back together
— rooms and shared shows. Not here, and not around a single `repository.AddAsync`.

## 5. Separate endpoints, one creation path

Separate endpoints, because three things that differ are static per action and cannot be branched inside
one handler:

- **authorisation** — `ArtistPermissions.ApplicationsSubmit` for apply, a new organiser permission for
  offer; `[HasPermission]` is an attribute, not a runtime choice;
- **rate limiting** — `[EnableRateLimiting(RateLimitPolicies.Apply)]` already differs per surface;
- **request shape** — an offer names its addressee artist and its slot; an application names neither.

Shared orchestration, because everything after actor resolution is identical. Both funnel into one
creation path so exclusivity, the tenant pair, the claim and the initiator are stamped the same way.
`ApplicationEntity.Create` stays the only constructor.

## 6. The one capability change this requires

Opportunity creation needs **no** new capability contract. It has no keyed dispatch at all today — one
code path serves all four deal types — and a targeted slot changes a column, not a behaviour.

The meaningful divergence is in the Application stage, and it is narrower and sharper than "the route":

`VenueHireApplyStep` validates **the artist tenant's** payment method
(`PaymentOperationReferences.MethodSetup(opportunityId, artistTenantId)`) before an application may
exist, because on a venue-hire deal the artist pays. Today that check sits at creation, which works only
because the artist is the creator. **In a direct offer the artist has done nothing at creation time**,
so the same check at the same place would reject every venue-hire offer.

The check does not belong to creation. It belongs to **the moment the artist side commits** — apply on
the public route, accept on the offer route, and a counter authored by the artist. So:

```csharp
internal interface IArtistCommitmentStep : IDealStep
{
    Task<UnitResult<ArtistCommitmentError>> EnsureAsync(
        ArtistCommitmentContext context,
        CancellationToken ct = default);
}
```

Keyed by `DealType` exactly as now, same two implementations, same registration table. The interface
*loses* work rather than gaining it: `StandardApplyStep.ApplyAsync` currently does nothing but call
`ApplicationEntity.Create`, which was never a strategy concern. Entity creation moves to the shared path
and the step becomes a pure precondition.

This is the only strategy-infrastructure change the problem requires.

## 7. Generics and unions — evaluated, not adopted

**Profile-typed generics (`IApply<Artist>` / `IApply<Venue>`) are the wrong discriminator.** Apply and
Offer create the same row with the same invariants. What differs is *authorisation* and *whose consent is
outstanding* — both runtime facts about a row, neither expressible as a type parameter. Concretely:

- `KeyedStrategyBuilder<TKey>` is `where TKey : struct, Enum` and single-keyed. A route-typed generic
  needs route × deal, a second registration dimension the builder cannot express without a Cartesian
  enum.
- A type parameter over the profile cannot describe a promoter organiser later, so it hardcodes the
  restriction this work exists to loosen.
- It replaces no runtime check. `ApplyAsync` resolves the acting profile at runtime either way.

**No union case is needed.** The union machinery (`KeyedUnionBuilder`, `DealUnionFactory`) has no
production consumer on `main` — only its own registration and `DealUnionBuilderTests`. This problem does
not create one: every entry path returns the same `ApplicationDto`, and the per-deal precondition returns
a uniform `UnitResult`. Adding a union here would be infrastructure justifying infrastructure. Where one
would eventually earn its keep is heterogeneous *results*, and the separate `/checkout` endpoints already
carry that today.

## 8. Terms, consent and counteroffers

Terms live on the Deal, and the Deal belongs to the Opportunity — which may have several applicants. A
counteroffer therefore cannot mutate the opportunity's deal.

**The Application gains a nullable `ProposedDealId`.** Effective terms are `ProposedDealId ?? the
opportunity's deal`. Public applications leave it null and behave exactly as today. A counter creates a
new deal row through `IDealModule.CreateAsync` and points at it.

**A counter may not change `DealType`.** The financial arrangement is the opportunity's; the negotiation
is over its numbers. This preserves every commitment reference, confirm step, contract factory and
settlement path unchanged. Switching arrangement is a decline plus a new offer, and saying so is a
product decision worth stating out loud rather than a limitation to hide.

**Consent is per side, stamped with what it signed.** `ArtistESignature` becomes two nullable slots —
one per side — each carrying the terms fingerprint that side signed. Accept requires both sides to have
signed a fingerprint equal to the effective terms' fingerprint now. That single invariant replaces
today's `TermsFingerprint != Calculate(deal, period)` guard and covers counteroffers with the same rule.
Neither side's signature is ever written by the other's action.

## 9. Targeted visibility

A `TargetedOffers` opportunity must not reach artist-facing discovery. Filter it out of the read
repository's discovery reads — match candidates, open-by-venue-tenant, the paged and unpaged
active-by-venue reads used by public surfaces — and reject `ApplyAsync` when admission does not permit
public applications. The addressed artist reaches the slot through the offer that names them, never
through browse.

## 10. Duplicates and concurrent acceptance

- **Duplicate offer or apply** — the existing unique `(OpportunityId, ArtistId)` index already forbids a
  second candidacy for the same pair. `OperationClaim` covers the retry that would otherwise create a
  second *slot*.
- **Concurrent acceptance** — unchanged and already correct. The filtered unique index
  `Applications(OpportunityId) WHERE State = Accepted` plus `RejectAllExceptAsync` decides the winner in
  the database. An offer is another row under that index, so "the artist accepts our offer" racing "we
  accept a rival application" needs no new mechanism. The loser is classified by the four-way
  `AttemptVerdict` taxonomy, not rerun.

## 11. The handoff into Booking is unchanged

`ApplicationAcceptanceSnapshot`, `AcceptedApplication`, `ContractSnapshot`, `BookingWorkflow.ConfirmAsync`,
every `IConfirmStep` / `IContractFactory` / `ICommitmentReferenceStep`, `BookingEntity`, `ContractEntity`
and the contract PDF are all untouched. Accept assembles the same snapshot from the two recorded
signatures and the effective deal; `ContractSnapshot` keeps naming them `ArtistSignature` and
`VenueSignature` because the row always has exactly one artist side and one venue side.

**Consequence worth stating: no published contract changes**, so this needs no producer PR, no package
publish and no platform version sync. It is one PR.

## 12. Phases

Each ends green and is independently reviewable inside the single delivery PR.

**Phase 1 — domain and persistence.** `Offered` state, `Counter` trigger, `Initiator`, per-side
signature slots with fingerprints, `ProposedDealId`, `OpportunityAdmission`. Re-scaffold initial
migrations from `api/`. *Gate:* Application and Opportunity unit suites, `has-pending-model-changes`
clean.

**Phase 2 — the artist-commitment precondition.** Narrow `IApplyStep` to `IArtistCommitmentStep`, move
entity creation to the shared path, invoke the step at every artist-side consent moment. *Gate:*
existing venue-hire apply behaviour unchanged; keyed-strategy architecture tests green.

**Phase 3 — application layer.** `OfferAsync`, `CounterAsync`; generalise Accept/Reject/Withdraw
authority to party-and-turn; admission checks; discovery filtering; new notification kinds. *Gate:*
Application integration suite.

**Phase 4 — API.** Offer, counter and decline endpoints; the organiser permission; both action shapes
gain their new links. *Gate:* Application API tests.

**Phase 5 — frontend.** Artist offers surface with accept/decline/counter; venue send-offer and
counter-review surfaces; the shared action labels. *Gate:* typecheck, Vitest, both UI suites.

## 13. Acceptance checks

- An organiser sends an offer to a named artist for a fixed-fee arrangement and for a revenue-share
  arrangement; the artist accepts each and a booking confirms with the existing money path.
- An offer alone creates no artist signature, no booking, no commitment and no charge — asserted, not
  assumed.
- The artist declines an offer; the organiser withdraws a different one; both reach the right terminal
  state and notify the right side.
- The artist counters; the organiser accepts the countered terms; the contract renders the countered
  numbers and both signatures name the same fingerprint.
- The organiser counters a public application; the artist accepts; same assertion.
- A counter that changes `DealType` is rejected.
- A stale consent — terms countered after one side signed — blocks acceptance.
- A targeted opportunity does not appear in artist discovery and rejects a direct apply.
- A retried offer request creates exactly one opportunity and exactly one application.
- Two acceptances race on one opportunity: exactly one booking exists, the loser is reported, not rerun.
- A venue-hire offer accepted by an artist without a payment method is rejected at accept, not at offer.
- All four existing financial workflows and their funding prerequisites behave as before.

## 14. Out of scope

Customer-configurable deals, a workflow builder, capability registries and versioned configuration.
Promoter tenants, multiple business profiles, multi-venue tenants, rooms and shared shows. Multi-leg
payment schedules. The organiser here is the opportunity's owning venue tenant; the domain says
"organiser" so that later work changes who resolves to it, not every call site.
