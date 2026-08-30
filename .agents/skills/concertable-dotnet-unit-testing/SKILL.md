---
name: concertable-dotnet-unit-testing
description: Concertable's test-tier gate, enforced by `api/TestConventions.targets` at build time — the project NAME is the only declaration of tier (ends `.UnitTests`/`.IntegrationTests`/`.ArchitectureTests`, contains `.E2ETests`, with EndsWith tested before Contains), a test project whose name states no tier fails the build, a support library sets `IsTestProject=false`, and the unit tier hard-fails on host packages (Mvc.Testing, TestHost, Respawn, Testcontainers, Playwright, Reqnroll), on the same symbols arriving transitively via `BannedSymbols.UnitTests.txt`, and on `Shouldly` — because the unit tier uses xUnit assertions, settled by the build rather than left open. Also the `AssemblyTrait("Category", ...)` that groups Test Explorer. Use when creating or renaming a test project here, when a unit test needs something a unit test cannot have, or when choosing an assertion library.
---

# Unit tests — the tier gate Concertable enforces at build time

The generic standard is the `unit-testing` skill: what makes a test a unit test, xUnit shape, naming,
building the SUT in the constructor, real collaborators over mocks. This file is the part a machine
decides here, so nothing about it is a judgement call at review time.

## The project NAME is the only declaration of tier

`api/TestConventions.targets`, imported once per service's `Directory.Build.targets`, derives
`ConcertableTestTier` from the project name alone — deliberately, so no separate property can disagree
with it:

| Name | Tier |
|---|---|
| ends `.UnitTests` | Unit |
| ends `.IntegrationTests` | Integration |
| contains `.E2ETests` | E2E |
| ends `.ArchitectureTests` | Architecture |

`EndsWith` is tested before `Contains`, or `Concertable.Payment.E2ETests.Helpers.UnitTests` — a unit-test
project for an E2E helper library — would resolve as E2E.

An `<IsTestProject>true</IsTestProject>` project whose name states no tier **fails the build**. A support
library that is not itself a suite sets `<IsTestProject>false</IsTestProject>`; the `*.Fixtures` and
`*.Helpers` projects are the precedents.

So the naming decision is the classification decision, and it happens before the file exists. Answer the
question the name asks: a test needing a host, HTTP or a database is an integration test.

## Two build errors a unit project cannot argue with

- **Host packages.** `Microsoft.AspNetCore.Mvc.Testing`, `Microsoft.AspNetCore.TestHost`, `Respawn`, and
  anything starting `Testcontainers`, `Microsoft.Playwright` or `Reqnroll` fail the unit tier as a
  `PackageReference`.
- **`Shouldly`.** One assertion library per tier, and **the unit tier uses xUnit assertions.** This is
  settled by the build, not an open call.

`BannedSymbols.UnitTests.txt` catches the same symbols arriving *transitively* — the usual shape here is a
unit project referencing a `*.Fixtures` or `*.Helpers` library, which a `PackageReference` scan cannot see.
`RS0030` fires on a symbol being **used**, so referencing the package is still allowed; touching it is not.

## Test Explorer groups by an assembly trait

Each test project carries its own `AssemblyInfo.cs` with
`[assembly: AssemblyTrait("Category", "Unit")]` (or `Integration`, and so on). Those are the intended
groupings; anything else appearing in VS Test Explorer is Reqnroll's generated traits or a stale cache —
the `reset-test-explorer` skill.

## Shared helpers

Any unit-tier helper lives in `Concertable.Testing` (general) or `Concertable.Testing.Unit`
(persistence), referenced by `ProjectReference` — Tests projects are carve-exempt.

### Repository

EF Core InMemory scaffolding: `Concertable.Testing.Unit`.

```csharp
private TestDbContext CreateContext() =>
    this.root.CreateContext<TestDbContext>(this.databaseName, options => new TestDbContext(options));
```

## The other tiers

Integration fixtures and the shared harness: `integration-testing`. E2E lives with the harness
it drives, in `api/Concertable.Shared/tests/Concertable.Testing.E2E/AGENTS.md` and its
`E2E_UI_CONVENTIONS.md` / `E2E_CONSIDERATIONS.md` siblings.
