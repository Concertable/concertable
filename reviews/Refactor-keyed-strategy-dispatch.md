# Docs review — Refactor/keyed-strategy-dispatch

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> ambiguous finding: flag it in one line, take the safe path, keep going.

**Reviewed up to commit:** `dc9b6a744c9196e088f6280b13b4ab9b45c34976`  _(2026-08-19)_

> Range reviewed: `1647ec6f8..88c368d39` (5 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **CON1 — MEDIUM — contradiction** — `plans/dotnet-11/DOTNET_11_ROADMAP.md:38`
  The plan and progress ledger made the .NET 11 work owner of the closed Deal cut-over's supported C# 15
  compiler/target matrix, but the roadmap item and dependency map omitted that handoff. Added the
  downstream gate and dependency edge.

## Incremental review — 2026-08-19

- [x] **ACC2 — HIGH — accuracy** — `plans/launch/DEAL_CLOSED_SUM_MODEL_PLAN.md:136`
  The generation trigger sits on an internal Application-layer factory interface even though the generated
  implementation and registration extension must compile in Infrastructure. In the real Deal module,
  mapper leaves live in `Concertable.B2B.Deal.Application`, updater leaves and the composition root live in
  `Concertable.B2B.Deal.Infrastructure`, and Application cannot reference or discover those outward
  Infrastructure types. A source generator only adds source to the compilation in which it runs;
  `InternalsVisibleTo` lets Infrastructure consume Application internals but does not make an Application
  annotation trigger generation in another compilation. Move the trigger to an Infrastructure-local anchor,
  keep the factory contract and marker in Application, generate the maps/factory/registrations in
  Infrastructure, and make Phase 0 prove that real two-project/`InternalsVisibleTo` topology.

- [x] **CON2 — HIGH — contradiction** — `plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PROGRESS.md:7`
  The new closed-Deal ledger routes its hard blocker through this owner ledger and draft PR #633, and this
  branch adds the reciprocal downstream handoff, but the owner ledger still identifies the old worktree,
  branch, and merged PR #625, says Phase 2 has not started, and directs `## Next Steps` to merge #625.
  PR #625 is already merged and #633 is the open Phase-2 PR, so the mandated return path leads to a
  completed action. Reconcile this ledger's identity, current state, completed work, verification, and next
  step to the live #633 workstream before registering it as the blocker owner.

- [x] **ACC3 — HIGH — accuracy** — `plans/launch/DEAL_CLOSED_SUM_MODEL_PLAN.md:225`
  The discovery contract says every annotated factory derives its net10 catalog from the contract cases,
  JSON attributes, `DealType`, and `DealEntity` agreement. The target Application module may reference only
  `Deal.Contracts`, so its Infrastructure compilation cannot see `Deal.Domain` or the entity hierarchy; a
  generator cannot inspect symbols absent from that compilation without breaking the module graph. Make
  Application generation derive the contract/JSON/enum catalog only, make Deal.Infrastructure validate the
  entity agreement and emit the entity selector, leave cross-module TypeScript/EF agreement to architecture
  tests, and prove those separate compilation scopes in Phase 0.

## Incremental review — 2026-08-19

- [x] **ACC4 — HIGH — accuracy** — `plans/launch/DEAL_CLOSED_SUM_MODEL_PLAN.md:245`
  The plan placed every concrete leaf in Infrastructure even though terms and mapper leaves belong to
  Application and only updater leaves belong to Infrastructure. The placement rule now keeps leaves in
  their natural layer while retaining the generated factory and DI registrations in Infrastructure.

- [x] **ACC5 — HIGH — accuracy** — `plans/launch/DEAL_CLOSED_SUM_MODEL_PLAN.md:292`
  Application-side diagnostics had no Application-visible discovery input because the only annotation
  was the Infrastructure-local generation anchor. The factory contract now carries a diagnostics-only
  Application annotation, distinct from the Infrastructure generation anchor, and Phase 0 proves both
  compilation paths.

- [x] **ACC6 — MEDIUM — accuracy** — `plans/launch/DEAL_CLOSED_SUM_MODEL_PROGRESS.md:86`
  The progress ledger described committed checkpoint `48d9a1f1f` as awaiting a commit and retained stale
  working-tree wording. It now records the committed incremental range and the current finding-fix state.

## Follow-up review — 2026-08-19

- [x] **ACC7 — MEDIUM — contradiction** — `plans/launch/DEAL_CLOSED_SUM_MODEL_PLAN.md:191`
  The C# 15 factory snippet omitted the diagnostics contract annotation although its accompanying text
  said only the input parameter changes. The snippet now retains the annotation.

## Final confirmation — 2026-08-19

No issues found. Checked accuracy vs reality, cross-doc contradiction, doc home and convention,
harness-reloaded concision, dangling references, and followable instruction through `dc9b6a744`.
