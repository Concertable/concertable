# Code review — Feature/ConfigConsolidation

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `42a37341fe9d5b7df48ac96759bbad6639bf43ae`  _(2026-07-17)_

> Range reviewed: `79904e92..42a37341` (1 commit).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **CV1 — LOW — C# conventions** — `api/Concertable.ServiceDefaults/Extensions.cs:62-65`
  Single-statement `if` body wrapped in braces:
  ```csharp
  if (envStream is not null)
  {
      shared.AddJsonStream(envStream);
  }
  ```
  `docs/CODE_CONVENTIONS.md` "Single-statement branches — no braces" mandates no braces here — drop
  them (`if (envStream is not null)\n    shared.AddJsonStream(envStream);`). Not enforced by
  `.editorconfig`, so it won't be auto-formatted. Everything else in the diff is clean.

_Reviewed all 5 lenses. Notes for context (not findings):_
- **Config precedence is correct.** `AddSharedDefaults` builds the embedded stream into a sub-config
  once and chains it via `ChainedConfigurationSource` at index 0 (lowest precedence), sidestepping the
  one-shot-stream re-read the inline comment names; `ShouldDisposeConfiguration = false` is required
  (a later `Sources` rebuild would otherwise dispose the shared config).
- **Behavior preserved.** Dropping `BlobStorage.ContainerName` from B2B.Web `appsettings.E2E.json` is
  safe — base `appsettings.json` still has `"images"`, the shared default now also supplies it, and
  the E2E `AppFixture` sets it in-memory at highest precedence.
- **Microservice isolation intact.** Each service keeps its own `DesignTimeConnectionString` copy
  (Auth/Messaging `internal`; B2B.DataAccess/Customer.Seed `public` for cross-assembly use) — no
  shared-across-boundary project, no data service reaching into another's runtime.
- **Customer helper in `Seed.Infrastructure`** is a pre-existing coupling (no new `.csproj` refs) and
  is already logged in `api/Concertable.Customer/TECH_DEBT.md`.
