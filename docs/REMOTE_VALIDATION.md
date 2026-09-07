# Remote validation — Concertable's gates

**The policy is the `remote-validation` skill**: who owns which gate, the delivery loop, resource
discipline, and the container-runtime pre-flight that must move real bytes before any local E2E run. What
is true of *this* repo:

| Gate | Concretely |
|---|---|
| Local worktree | required generators and invariant greps, the smallest affected project or app build, focused unit tests |
| Draft-PR CI | `build`, `carve-*`, `unit-tests`, `integration-tests`, on the exact remote head |
| Merge queue | `e2e-api-tests` + `e2e-ui-tests`, at the tier the `merge` skill's Step 4 selects |

- **Never `dotnet build api/Concertable.slnx`**, every unit project, or the full integration matrix as
  routine local verification.
- While one worktree's diagnostic run owns Docker/Testcontainers, the other worktrees keep implementing or
  wait for remote checks.
- Local E2E runs only through `./scripts/e2e.ps1` and its `e2e-*` skills, and only to diagnose a queue
  failure — root [`AGENTS.md`](../AGENTS.md).

## The debug loop — the order that avoids a 40-minute round trip

- **E2E runs only on the `merge_group` event** (`test.yml:183`; jobs at `:908` and `:1004`), never on the PR.
  A green PR is silent about E2E, and it does not show in `gh pr checks`. The queue is where E2E speaks, and
  a queue failure re-spends the whole cycle — so run the affected E2E suites locally *before* enqueuing.
- Failing test → its `e2e-*`/`integration-debug` skill → fix → re-run **only that test**. Never re-run a
  suite to reconfirm failures you already know about.
- Before pushing, run the gate that covers what changed, picked by what *consumes* the change rather than by
  the files you opened: a signature → whole-solution build; a package or reference-graph edit → that
  service's `ArchitectureTests`; a `src/` edit → that service's unit + integration.
- `local-platform prepare` caches on a content hash of `api/` + `local-platform.ps1`: unchanged inputs skip
  the ~15-minute pack in about a second. Repack deliberately with `prepare --force`.
- **Never abort a `prepare` mid-run.** It deletes the package root before packing, so an abort leaves zero
  packages and costs a full repack.
- One E2E application at a time — the host-capacity gate and a single shared Stripe test account.
- `main` is behind a merge queue, so it owns the strategy: no `--squash`/`--merge` flag, and
  `gh pr merge <n>` enqueues rather than merging.

## Exceptions — delivery gates with no remote feedback yet

Some work cannot obtain meaningful remote feedback until a package is published or a
`chore/platform-sync-*` PR exists. Follow `/package-cutover` and the platform-sync flow for those gates. A
targeted local build against an exact producer artifact is still valid; it does not restore a
full-local-gate default.

Historical plan/ledger evidence remains historical. Reconcile only outstanding instructions to this policy.
