# Reunion alpha.2 package baseline plan

> Next steps live in @plans/typed-result/REUNION_ALPHA2_BASELINE_PROGRESS.md → `## Next Steps`.

## Outcome

Align every existing Concertable reference to `Reunion`, `Reunion.Validation`, `Reunion.Errors`, and
`Reunion.AspNetCore` to `0.1.0-alpha.2`, verify each independently owned service closure, and deliver
the package-only cutover through the normal PR, publication, and platform-sync lifecycle.

## Producer contract

- NuGet.org publishes `0.1.0-alpha.2` for all four packages.
- `Reunion`, `Reunion.Validation`, and `Reunion.AspNetCore` packages come from producer commit
  `ab3386a76e`; `Reunion.Errors` comes from `1500270cc`.
- The baseline includes structured `ValidationResult`, direct static `ErrorDefinition` factories,
  canonical native-union source/API baselines, and target-typed raw-payload plus exact named-case
  conversions across `Result`, `UnitResult`, and `Option`.

## Scope

- Inventory every existing direct Reunion package reference on current `origin/main`.
- Change each existing package version to exactly `0.1.0-alpha.2`.
- Preserve usage-based ownership: do not add unused packages or redistribute dependencies through
  Shared projects.
- Compile and test every affected service independently. This plan owns version alignment only;
  service-owned semantic migrations and broader construction refactors stay in their existing plans.

## Phases

### Phase 1 — Current-main inventory

- Create the reserved worktree from fresh `origin/main`.
- Record all direct package references and the affected standalone service closures.
- Confirm no red platform-sync gate or overlapping package-cutover owner exists.

### Phase 2 — Exact package alignment

- Update every existing Reunion-family package version to `0.1.0-alpha.2`.
- Restore each affected project and prove the resolved graph contains no alpha.1 Reunion package.
- Make only the minimal source corrections required by the alpha.2 API; return semantic work to the
  owning service ledger.

### Phase 3 — Verification and delivery

- Run affected Release builds, focused unit tests, and architecture checks locally; push the coherent
  checkpoint and require exact-head PR CI for standalone carves and complete unit/integration matrices.
- Run review and PR preflight, commit, push, and merge through the normal code path.
- Follow the generated platform-sync PR to green and merged; repair any affected consumer in that PR.

## Definition of done

- Every existing Reunion-family reference resolves exactly `0.1.0-alpha.2`.
- Repository scans find no alpha.1 Reunion pin and no newly introduced unused package reference.
- Every affected service remains independently buildable from its published package closure.
- The source PR and generated platform-sync PR are terminal and green.
