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

### A change to the dev seed path has no gate cheaper than the merge queue

`IDevSeeder` runs in dev and E2E; `ITestSeeder` runs in integration. Nothing else executes the dev path, so a
defect in a dev seeder, `DevDbInitializer`, or `SeedingIdentityInterceptor` is invisible to `unit-tests` and
`integration-tests` and first appears as `e2e-api-tests` in the merge queue — roughly 40 minutes per
iteration, reported as ten health-check timeouts rather than as the seeder that actually threw.

So a change touching that path earns a local pre-flight before enqueueing, under the same reasoning as the
package gates above: it is not a full-local-gate default, it is a gate with no cheaper remote feedback. Run
`./scripts/e2e.ps1 api b2b` and confirm the seeders complete before pushing for the queue. Read the seeder
outcome directly — `api/Concertable.B2B/tests/E2ETests/Concertable.B2B.E2ETests/api-tests.last.log` carries
`Seeder <name> failed` and `b2b-web: Finished`, while the wrapper script only prints its summary at the end.

This cost three separate merge-queue cycles on PR #633, each one surfacing the next seeder failure that the
previous one had masked.

Historical plan/ledger evidence remains historical. Reconcile only outstanding instructions to this policy.
