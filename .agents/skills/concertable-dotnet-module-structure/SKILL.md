---
name: concertable-dotnet-module-structure
description: Concertable's module project naming and boundary decisions — project names carry the owning service while genuinely cross-service shared libraries stay unprefixed under `Concertable.Shared`, controllers are internal by default through `InternalControllerFeatureProvider` with three deliberate public exceptions, the cross-module `ReadDbContext` is deleted, and the shared reference vocabulary is `Genre`. Use when creating a project here, naming one, or making a controller public.
---

# Modules — Concertable's project naming and boundary decisions

The generic standard is the `module-structure` skill: the Contracts/Domain/Application/Infrastructure/Api
split, the inward-only reference graph, the visibility cascade and `InternalsVisibleTo`, the cross-module
rules, and what a module facade may do. It governs every module and shared library here.

A service is a modular monolith *internally*, which is what these rules cover. They are not a description of
one monolith spanning services — that boundary is
[`SERVICE_BOUNDARIES.md`](../../standards/dotnet/structure/SERVICE_BOUNDARIES.md).

## Project names carry the owning service; shared libraries do not

```text
api/Concertable.<Service>/src/Modules/<Module>/
  Concertable.<Service>.<Module>.{Contracts,Domain,Application,Infrastructure,Api}/
  Tests/Concertable.<Service>.<Module>.{UnitTests,IntegrationTests}/
```

`Concertable.B2B.Concert.Domain`, `Concertable.Customer.Review.Application`. Genuinely cross-service shared
libraries sit at `api/Concertable.Shared/Concertable.<Name>/` **unprefixed**.

## Controllers are internal unless deliberately public

Internal controllers are discovered by `InternalControllerFeatureProvider`
(`Concertable.Shared.Api/Controllers/`), wired by `ControllerBuilderExtensions`. The three deliberately
`public` controllers are `BlobController`, `FallbackController` and `GenreController`.

## What the cross-module rules resolved to here

The cross-module `ReadDbContext` that existed during extraction has been **deleted**. The shared reference
vocabulary the skill's enum rule refers to is `Genre`, in `Concertable.Contracts` — see
[`../CONTRACTS.md`](../../standards/dotnet/CONTRACTS.md).
