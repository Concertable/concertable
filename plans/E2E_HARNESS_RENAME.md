# Rename `Concertable.E2ETests` → `Concertable.Testing.E2E`

> Cosmetic-but-cross-cutting rename of a shared **test harness** project. Not boundary-blocked (test
> projects aren't published packages — everything references it by ProjectReference), so it's a single
> PR, but it touches many files. Do it on a `Refactor/E2EHarnessRename` branch off main, after
> `Feature/BookingAgreement` lands. Sits alongside `plans/PDF_RENDERER_RENAME.md` / `plans/b2b/DEAL_RENAME.md`.

## Why

`api/Concertable.Shared/tests/Concertable.E2ETests/` contains **no tests** — no `[Fact]`, `[Scenario]`,
or `.feature`. It's the shared E2E **harness**: `HealthWaiter`, `PollingService`, `TestTokenMinter`,
`DistributedApplicationBuilderExtensions`, `AspireResourceLogger`, `RespawnableDb`, the MSBuild tasks.
The actual E2E tests live in `Concertable.B2B.E2ETests` / `Concertable.Customer.E2ETests`.

Its siblings in the same `tests/` folder establish the convention for shared harness libs:
`Concertable.Testing` (helpers) and `Concertable.Testing.Integration` (integration harness). The E2E
harness is the exact peer of `Concertable.Testing.Integration`, so it should be
**`Concertable.Testing.E2E`**. The current name reads like a test suite and misleads every reader.
(`Concertable.Kernel.UnitTests` in the same folder is fine — it's a real suite testing the Kernel, not
a harness.)

## Touch-points (audit before renaming; grep `Concertable.E2ETests` excluding `*.B2B.*/*.Customer.*/*.Payment.*/*.Search.*` suites)

- **The project itself** — folder, `Concertable.E2ETests.csproj`, `<AssemblyName>`/`<RootNamespace>` if
  set, `GlobalUsings.cs`, and every `namespace Concertable.E2ETests…` / `using Concertable.E2ETests…`
  inside it (incl. `Support/`).
- **`ProjectReference`s** from the consumers: `Concertable.B2B.E2ETests`(+`.Ui`),
  `Concertable.Customer.E2ETests`(+`.Ui`), `Concertable.Payment.E2ETests.Helpers`,
  `Concertable.Search.E2ETests.Helpers`, and their `using Concertable.E2ETests;`.
- **`InternalsVisibleTo("Concertable.E2ETests")`** in production `AssemblyInfo.cs` — at least
  B2B `Concert.Api`/`Concert.Application`/`Contract.Application`, Customer `Ticket.Application`,
  `Payment.Application`. Update the assembly name string. *(Side-note worth a glance during the rename:
  production Application granting internals to the shared harness is in tension with the harness's own
  "nothing service-specific here" rule — confirm it's actually needed, don't just carry it forward.)*
- **Solution** — `api/Concertable.slnx`.
- **Scripts** — `e2e.ps1`, the project's `ui-trace.ps1`.
- **Docs** — `api/docs/TESTS.md`; the four `.claude/skills/e2e-*/SKILL.md`; the shared E2E `CLAUDE.md`
  plus the two per-suite pointer docs that link to it
  (`Concertable.B2B.E2ETests.Ui/CLAUDE.md`, `Concertable.Customer.E2ETests.Ui/CLAUDE.md` — fix the
  relative path in their links).
- **Baseline** — `E2E_BASELINE.md` moves with the folder; update any path references to it in the
  scripts/skills above.

**Do NOT touch** `.claude/worktrees/agent-*` — those are throwaway agent git worktrees carrying a stale
`api/Tests/…` layout; they're not part of the real tree.

## Phases

### Phase 1 — Rename the project + its internals
- Rename folder + csproj + assembly/namespace; fix intra-project `namespace`/`using`.
- **Gate:** the project builds in isolation.

### Phase 2 — Repoint every consumer and reference
- Update ProjectReferences, `using`s, `InternalsVisibleTo`, `.slnx`, scripts, docs, skill paths.
- **Gate:** `dotnet build api/Concertable.slnx` green (0 errors); `./scripts/e2e.ps1 ui regress` green (proves
  the harness still wires up and the suites resolve it); skill docs point at the new path.
- `git rm` this plan in the completing commit.

## Not in scope
- The E2E **suites** (`Concertable.B2B.E2ETests`, `Concertable.Customer.E2ETests`, `.Ui`) — they hold
  real tests, their names are correct.
- `Concertable.Kernel.UnitTests` — correctly named suite.
- The fast-forward scenario cleanup — separate plan (`plans/E2E_FAST_FORWARD_REFACTOR.md`).
