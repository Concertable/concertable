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

## Exceptions — delivery gates with no remote feedback yet

Some work cannot obtain meaningful remote feedback until a package is published or a
`chore/platform-sync-*` PR exists. Follow `/package-cutover` and the platform-sync flow for those gates. A
targeted local build against an exact producer artifact is still valid; it does not restore a
full-local-gate default.

Historical plan/ledger evidence remains historical. Reconcile only outstanding instructions to this policy.
