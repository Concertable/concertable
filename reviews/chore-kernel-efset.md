# Code review — chore/kernel-efset

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `33a9ed9a46a836c686e84a2f582a498c7fdbe715`  `(2026-09-02)`
**Judgment:** `approved`

## Review pass — 2026-09-02 — full

**Candidate base:** `b91ed63bf4c14484805a99db30b074fe0a90a646`
**Candidate head:** `33a9ed9a46a836c686e84a2f582a498c7fdbe715`
**Candidate branch:** `chore/kernel-efset`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:3af4faa13b39fb981b6f7a1659acdc7323cc09bfc918635c95968900f253e70d` `(2 paths)`
**Work-order path:** `reviews/chore-kernel-efset.md`
**Work-order mode:** `new`
**Pass judgment:** `approved`

### Findings

No findings.

- **native/general** (correctness, reuse, simplification, efficiency, error handling): no findings.
  Verified the explicit `IList<T>` members (order/dedup-breaking `Insert` + indexer-set throw; the rest
  delegate to the backing list), the `IReadOnlySet<T>` delegation via a fresh `[.. items]` snapshot,
  the constructor routing every value through `Add`, the `GetEnumerator` returning the list enumerator,
  and the C# 14 `extension<T>(IReadOnlyCollection<T>) where T : struct` block binding `new(values)` to
  the intended constructor. Constructor dedup is O(n²) but `EfSet` instances are small and not a hot
  path. `where T : struct` is the correct constraint — EF primitive-collection element types are value
  types, enums already satisfy `struct`, and it excludes `Nullable<T>`.
- **microservice-boundaries**: `Concertable.Kernel` is the universal shared tier and already references
  `Microsoft.EntityFrameworkCore` + hosts `EntityTypeBuilderExtensions`; a generic EF-persistence
  primitive with no audience-specific members is the intersection, not the union. Correct home.
- **csharp-style / csharp-naming**: fields unprefixed, no unnecessary `this.`, type `sealed`, extension
  in a C# 14 `extension()` block, `EfSet` names the type by what it is (a set) with the EF rationale in
  the doc-comment rather than the identifier.
- **unit-testing**: `EfSetTests` is a pure in-memory test of a value type — correct tier. Covers ctor
  dedup + order, `Add` dedup, `Remove`/`Clear`, the `IReadOnlySet` operations, positional-mutation
  throwing, and `ToEfSet`. Remote-persistence behaviour was proven separately by the B2B integration
  matrix on the concrete equivalent (`GenreSet`) and the `EfSet<Genre>` proof-of-concept.

Addressed during the pass: the native reviewer noted the public `Remove`/`Clear` had no direct test;
`33a9ed9a4` adds both.
