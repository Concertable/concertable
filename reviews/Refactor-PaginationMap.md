# Code review — Refactor/PaginationMap

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `606eb2f7e9596510346dbeffa30c9a274aaf63a5`  _(2026-08-16)_

> Range reviewed: `27b0195d1..HEAD` (3 commits + review fixes).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **CV1 — LOW — conventions (Lens E)** — `api/Concertable.Shared/src/Concertable.Contracts/PaginationExtensions.cs:6`
  The moved method carried the legacy `public static … (this X x)` signature across.
  `api/agents/CODE_CONVENTIONS.md` "Extension members": *"New extension members go in
  `extension(Receiver)` blocks… Never add a new legacy `public static … (this X x)` method; the existing
  ones await a migration sweep."* A move that renames the method is a new declaration, so the rule
  applies. **Fixed:** now a C# 14 generic `extension<TSource>(IPagination<TSource> source)` block,
  matching the repo's own recent `ControllerBuilderExtensions.AddApplicationJson`.

- [x] **NAT1 — LOW — correctness** — `scripts/unit.ps1:41`
  The new test project was absent from `$sharedProjects`, so `./scripts/unit.ps1 shared` and
  `./scripts/unit.ps1 run` silently skipped it. CI's `*.UnitTests.csproj` glob still discovers it, so
  this was local-only blindness — the worse kind, because it makes a local run look green while
  covering less than CI. **Fixed:** added to the array.

- [x] **NAT2 — LOW — test coverage** — `.../Concertable.Contracts.UnitTests/PaginationExtensionsTests.cs:19`
  `Map_CarriesThePagingMetadataAcross` asserted `TotalCount`, `PageNumber` and `PageSize` but omitted
  `TotalPages` — the one metadata member `Map` does **not** copy, because `Pagination<T>`'s constructor
  re-derives it from `totalCount`/`pageSize`. That makes it exactly the member a future rewrite of `Map`
  would get wrong while the other three still passed. **Fixed:** asserts `TotalPages == 19` (57/3).

### Checked and clean

- **`Map` faithfulness:** the body is unchanged from the old `Select`, and `Pagination<T>` re-derives
  `TotalPages` identically, so there is no behavioural difference — including the empty-page case, which
  a test now pins.
- **No in-repo breakage from the removal:** the only caller (`ModerationService:28`) reaches the old
  `Select` through a **PackageReference** pinned to the current platform version, not a ProjectReference,
  so the solution and the B2B carve still compile. That call migrates to `Map` in the follow-up sync PR,
  which is the whole point of a publish-first cut-over.
- **Contracts placement/packaging:** `Map` adds no dependency (`System.Linq` plus the co-located
  `Pagination<T>`), `Concertable.Contracts` is already `IsPackable`, and a page projection is
  audience-agnostic — the intersection rule in `api/AGENTS.md` is satisfied.
- **Test project wiring:** the csproj mirrors the sibling `Concertable.Kernel.UnitTests` template, the
  `*.UnitTests.csproj` name means CI discovers it with no workflow change, and the `.slnx` entry sits in
  the right group.
- **Doc trail is deliberately unchanged:** `api/TECH_DEBT.md` and `api/agents/CODE_CONVENTIONS.md` still
  describe `Select` and the move as pending. Both are correct for the *current* pin, and the TECH_DEBT
  entry's own "Resolves when" covers the call-site migration — so they belong to the sync PR that
  actually completes the cut-over, not to this one.
