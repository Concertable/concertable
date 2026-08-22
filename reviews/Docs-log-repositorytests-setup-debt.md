# Docs review — Docs/log-repositorytests-setup-debt

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> ambiguous finding: flag it in one line, take the safe path, keep going.

**Reviewed up to commit:** `91560ba127875fb3e528892bc6fc1180790fe666`  _(2026-08-22)_

> Range reviewed: `f175eaae7..91560ba12` (1 commit).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

No findings. Single-file addition to `api/Concertable.DataAccess/TECH_DEBT.md`, one new entry.

- Lens A (accuracy) — the entry's claims verified against the actual file it describes:
  `RepositoryTests.cs` does repeat `var root = new InMemoryDatabaseRoot(); var databaseName =
  Guid.NewGuid().ToString();` at the top of every database-touching test, and the three named
  reflection-only tests (`Repository_ContextField_UsesCombinedCapabilityOnly`,
  `WriteRepository_ContextField_UsesWriteCapabilityOnly`, `ReadRepository_ContextField_UsesReadCapabilityOnly`)
  genuinely touch no database. No dead references — no links or `@`-includes added.
- Lens B (contradiction) — no other doc states or implies the opposite; no sibling debt entry in this
  file or `api/TECH_DEBT.md` covers the same ground.
- Lens C (right home) — this is `RepositoryTests.cs`-local debt, correctly homed in
  `api/Concertable.DataAccess/TECH_DEBT.md` (the file already owning every other DataAccess-area entry),
  not bolted onto a hub.
- Lens D (concision) — `TECH_DEBT.md` is not a harness-reloaded file; N/A.
- Lens E (dangling references) — no plan filename, phase number, or ticket cited.
- Lens F (followable instruction) — the "Resolves when" condition is concrete and self-contained.
