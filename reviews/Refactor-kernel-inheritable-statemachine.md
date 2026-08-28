# Code review — Refactor/kernel-inheritable-statemachine

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `56e16d02381c4b34a74e5782d40ca33e35ddda44`  `(2026-08-28)`
**Judgment:** `approved`

## Review pass — 2026-08-28 — full

**Candidate base:** `5b9e20e7723aadc3813548ac833a339b1652b23b`
**Candidate head:** `56e16d02381c4b34a74e5782d40ca33e35ddda44`
**Candidate branch:** `Refactor/kernel-inheritable-statemachine`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:8ffb9cb2e5624671e915ace303d395997a243b997dfa655e7cfc3064531ee616` `(2 paths)`
**Candidate bundle:** `C:/Users/TOMMYS~1/AppData/Local/Temp/claude/C--Users-TommySeery-source-repos-Concertable/21880d84-86de-49a4-b8c6-0ffec730b4e3/scratchpad/review-bundle-pr851`
**Candidate bundle identity:** `sha256:a5bf2083f92f9ba523e231fa1ed9be400bae94f058fc1fb53a4283d3e9aee7f1`
**Work-order path:** `reviews/Refactor-kernel-inheritable-statemachine.md`
**Work-order mode:** `new`
**Pass judgment:** `approved`

### Scope

Two files, 12 lines: `Concertable.Kernel.StateMachine<TState,TTrigger>` un-sealed with a `protected`
constructor, plus a new sealed `ConfiguredStateMachine<TState,TTrigger>` for direct edge-list
construction; `StateMachineTests` updated to construct the new concrete type.

### Rules manifest

Routed by `.agents/hooks/skill_router.py` for the exact changed paths: `dotnet-standards:microservice-boundaries`,
`dotnet:microservice-boundaries`, `dotnet-standards:unit-testing`, `dotnet:unit-testing`. Also applicable as
the naming/style floor: `dotnet-standards:csharp-style`, `dotnet-standards:csharp-naming`.

- microservice-boundaries — shared code (Kernel) must stay the intersection of what every service needs;
  this change adds a generic capability (inheritability), no audience-specific member. Compliant.
- unit-testing — xUnit shape, constructor-built SUT, real collaborators retained unchanged; only the
  constructed type name changed. Compliant.
- csharp-style — `ConfiguredStateMachine`'s primary constructor forwards only to `base(...)` and captures
  nothing, matching the "pure base-forwarding leaf type" exception to explicit fields. Compliant.
- csharp-naming — no suffix-table violation; not a DI-injected collaborator.

No path in this candidate matches the merge gate's `security_paths` inventory — no security layer required,
no security marker.

### Findings

None. Contract and behavior verified unchanged (same `Transition` signature, duplicate-edge
`ArgumentException`, immutable snapshot, concurrent-read safety) — proven by `Concertable.Kernel.UnitTests`,
246/246 passing against the new type.
