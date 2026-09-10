# Collection-abstraction architecture gate

## Context — what happened

While implementing tenant-verification Phase 2 (`Feature/launch_tenant-verification`, PR #784), an
internal service method returned `List<VerificationDocumentEntity>` instead of
`IReadOnlyList<VerificationDocumentEntity>` — a violation of the `result-carriers` skill's stated rule
("Zero or more values | `IReadOnlyList<T>`"). The rule was read earlier in the same session, then not
re-applied to that method. A full two-layer code review (native + security) also missed it: neither layer
checks a diff against this repo's own convention docs at that granularity.

Enabling Meziantou.Analyzer's `MA0016` ("prefer collection abstraction over implementation") as
`dotnet_diagnostic.MA0016.severity = error` was tried as a mechanical fix and reverted. Two problems:

1. **MA0016 only fires on `public`/`protected` members.** This codebase is internal-by-convention
   (`module-structure` skill: controllers, services, repositories are `internal` by default), so MA0016
   would rarely fire on the code this rule actually needs to catch — it does fire across
   project-reference assembly boundaries within one service (e.g. a `public` Domain entity property is
   visible from that service's Application/Infrastructure/Api assemblies even though the type never
   leaves the service), which is *some* real coverage, just not the internal/private case that caused
   this incident.
2. **Turning it on surfaced 4 pre-existing violations**, three of which were EF Core navigation
   collection properties (`ConcertEntity.Genres`, `OpportunityEntity.Genres`/`Applications`,
   `ArtistEntity.Genres`) — not casual mistakes, but places where the concrete type carries real meaning
   (`OpportunityEntity.Applications` is `HashSet<ApplicationEntity>` specifically so `.Add()` silently
   rejects a duplicate application; changing it to `ICollection<T>` would have silently dropped that
   invariant). One of the two `Genres` fixes was also picked wrong on the first attempt
   (`ISet<Genre>` — not even collection-expression-constructible) before being caught.

**The lesson:** a blanket type-swap in response to an analyzer is exactly the kind of change that needs
the same rigor as any other production change — check every actual usage (reads *and* writes) before
assuming a "just an abstraction" edit is safe. Two related tech-debt entries were logged instead of forcing
the swap through: `api/Concertable.B2B/src/Modules/Concert/TECH_DEBT.md` and
`api/Concertable.B2B/src/Modules/Artist/TECH_DEBT.md` (`Genres` should be set-shaped to prevent duplicate
tags, but needs verifying `HashSet<T>` actually works with EF Core's `PrimitiveCollection` JSON-column
mapping first — `ICollection<T>` has a known query bug there, dotnet/efcore#35502).

## The actual problem to solve

Get a **mechanical, zero-token, all-visibility** gate for "a method/property should not return a concrete
`List<T>`/`Dictionary<TKey,TValue>`/`HashSet<T>` directly" — the `result-carriers` skill's own rule, which
currently has no enforcement below `public`/`protected` (MA0016) and no enforcement at all above
"a human/agent happens to re-check the diff."

## Proposed solution

An `ArchUnitNET` architecture test (`TngTech.ArchUnitNET.xUnit` is already a pinned package in this repo,
used for the existing `*.ArchitectureTests` projects). Unlike a Roslyn analyzer, ArchUnit inspects the
**compiled assembly via reflection**, so it sees `internal` and `private` members the same as `public`
ones — this is the coverage MA0016 cannot provide here.

Shape, following this repo's own self-verifying-allowlist pattern (`unit-testing` skill: "An
architecture-guard allowlist must verify itself" — a `public static TheoryData<>` feeding a `[Theory]`
that asserts each allowlisted item *still violates* the rule, so a stale entry fails on its own):

```csharp
// Fails: any method or property whose declared type is exactly List<>, Dictionary<,>, or HashSet<>
// (not IList/ICollection/IReadOnlyList/etc. — those are fine), across every visibility.
// Allowlist: EF navigation properties where the concrete type is load-bearing (e.g. dedup-on-Add,
// PrimitiveCollection JSON mapping quirks) — each entry names the exact member and the reason.
```

## Next steps

1. **Survey first, across all five services** (B2B, Payment, Customer, Search, Auth) — count and list
   every method/property returning `List<T>`/`Dictionary<TKey,TValue>`/`HashSet<T>` directly. Do not skip
   this: the MA0016 attempt above found real, intentional exceptions on the first try in a codebase this
   agent had only just started working in.
2. For each hit, decide: **fix** (return type changes to the matching interface, no behavior depends on
   the concrete type) or **allowlist with a named reason** (something depends on Add-time dedup, ordering
   guarantees, a serialization/EF-mapping quirk, etc.) — never allowlist without checking actual usages
   (reads *and* writes) first.
3. Write the ArchUnit test with the self-verifying allowlist shape above, seeded from step 2's decisions.
4. Land it as its own PR — this is independent of any other in-flight work and touches every service, so
   it should not ride along with an unrelated feature branch.
5. Delete this file once the test is landed and enforcing.
