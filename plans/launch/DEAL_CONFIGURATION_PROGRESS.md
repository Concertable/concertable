# Configurable Deal progress

- Plan: `plans/launch/DEAL_CONFIGURATION_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/deal-configuration`
- Worktree: none; planning-only work uses the normal checkout
- Branch: no implementation branch; suggested `Refactor/deal-configuration`
- PR: none for implementation
- Dependency/package gates: typed-language work does not wait for Postgres or .NET 11; B2B `jsonb`
  persistence delivery requires the B2B Postgres cut-over. Refresh live Deal/commission owners first.
- Last reconciled: `2026-09-10` against `17e03aca1` (`origin/main`)

## Current state

Design selected; implementation has not started. Hybrid relational metadata/capabilities plus a typed
`jsonb` rule graph replaces permanent whole-Deal subtype identity. Four platform presets remain the MVP;
tenant authoring, feature entitlements and the visual builder are future product work. The representation
refactor is not an additional launch gate. Finbuckle is not a dependency.

## Next Steps

No runtime work is authorized by this planning update. When implementation is requested:

1. Resolve current owner branches and consume their landed APIs; do not resume an old mapper/updater
   or lifecycle-interface design from prose.
2. Start Phase 1 in its own implementation worktree from current `origin/main`: define the finite
   language/compatibility contract and integrate the evaluator through the four existing consumers.
3. Record the exact module/public consumer matrix and prove four-preset equivalence before moving to
   revision storage. Reconcile the provider delivery gate before Phase 2.

## Verification

- Design checked against Deal architecture, current terms/TPT mapping, lifecycle/dispatch/provider plans,
  the active commission baseline and the existing tenant-context/membership model.
- Planning validation and review are recorded in the documentation delivery's canonical work order.
- No runtime changes or runtime test claims belong to this planning checkpoint.

## Reviews

- Documentation delivery requires isolated review of the committed design and reconciled plans.

## Decisions and blockers

- One model for supplied and future custom configurations; no permanent `Composite` exception.
- Economic compatibility is code-owned validation; tenant eligibility is separate future authorization.
- Explicitly platform-owned presets avoid fake tenancy and premature per-tenant duplication.
- Contract snapshots include effective rules and capability versions, not just source references.
- New supported combinations are data; new language/effect semantics require deployed code.
- No current implementation blocker is claimed: implementation is not started. Provider and owner
  integration are phase-specific delivery gates, not reasons to block all design or typed-language work.
- Documentation debt: the architecture's detailed four-case walkthrough retains historical symbol
  examples. Phase 1 must reconcile that walkthrough with the landed vocabulary/lifecycle owners;
  done means its concrete paths/interfaces match those consumers without restoring deleted abstractions.

## Integration handoffs and owner classification

- **New plan + ledger:** this owner carries the configurable product and tenant/MVP split.
- **Plan changes:** Deal architecture, Deal dispatch's later closure target, lifecycle selection target,
  Postgres follow-on and .NET 11 downstream assumptions. Dispatch/provider ledgers record the resulting
  return path without pretending source merge alone proves another owner's complete delivery.
- **Roadmap:** add `launch/deal-configuration` as architecture work, not a launch gate, and explicitly
  defer tenant authoring/eligibility and visual editing.
- **Live commission owner — handoff, not competing edits:** PR
  [#847](https://github.com/Concertable/concertable/pull/847),
  `Feature/launch_platform-commission-phase2`, owns its current four pure gross calculators and Payment
  binding integration. At its next substantive checkpoint, record that those formulas/results remain
  the launch baseline; after that integration this plan can replace preset-keyed selection with the
  validated evaluator. Do not block commission delivery on this plan or add tenant commission rates.
- **Live Deal vocabulary owner — handoff, not competing edits:**
  `Refactor/layering_deal-vocabulary-and-mapper-collapse` in
  `.worktrees/Refactor-layering_deal-vocabulary-and-mapper-collapse` owns Deal vocabulary/layering and
  removal of mechanical mapper DI. No PR existed at reconciliation. Consume its landed domain/API
  boundaries; do not restore the removed mapper/updater families to satisfy an old dispatch plan.
- **Lifecycle operation-claims owner — no implementation change:**
  `Refactor/launch_operation-claims-and-attempts` owns its live lifecycle ledger changes. Preserve its
  acceptance transaction/claims; this plan will integrate pinned revision checks at that boundary.
- **No change:** tenant PRS/VAT/payment-term defaults, tenant verification, membership/active-tenant
  infrastructure, Payment provider ownership, lifecycle read projections and legal drafting keep their
  existing owners. None is repurposed as a deal-builder or Finbuckle migration. Seal enforcement remains
  its own owner; revision-child immutability must be proven by this plan's persistence gate.
