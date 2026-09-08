# Concertable.B2B

The **B2B** service of [Concertable](https://github.com/Concertable/concertable) — the
business-facing side of the marketplace, where venue and artist managers register, manage their
venues/artists, and create concerts. It is a *data service*: it owns its data and talks to other
data services (Customer, Search) only through `*.Contracts` integration events, never their runtime.
It depends on the **Auth** and **Payment** adapter services at runtime.

## Source ownership during extraction

Follow the [current system ownership map](https://github.com/Concertable/docs/blob/main/SYSTEM.md)
and the extraction owner's qualified source/release state. Do not assume the retained
[`Concertable/b2b`](https://github.com/Concertable/b2b) checkout includes every merged monorepo change.
At the 8 September 2026 check, the monorepo B2B source contains PR633's lifecycle architecture and the
retained repository's main does not; the monorepo source remains authoritative for that work.

## Implementation planning

[Commercial execution planning](plans/commercial/COMMERCIAL_ROADMAP.md) belongs to B2B and travels with
its source. Product decisions remain in Concertable/docs; external services retain their own plans.

## Building standalone

The deployable closure consumes Concertable's shared platform and cross-service contracts as NuGet
`PackageReference`s from the private org feed `https://nuget.pkg.github.com/Concertable`. Restoring
them needs a GitHub [personal access token](https://github.com/settings/tokens) with the
**`read:packages`** scope, exported as `GITHUB_PACKAGES_TOKEN` (the `nuget.config` reads it):

```sh
export GITHUB_PACKAGES_TOKEN=<your read:packages PAT>
dotnet build src/Concertable.B2B.Web/Concertable.B2B.Web.csproj
dotnet build src/Concertable.B2B.Workers/Concertable.B2B.Workers.csproj
```

Building the two host projects pulls the whole deployable closure. (In the monorepo's CI the same
variable is supplied by the workflow's `GITHUB_TOKEN`; standalone, you export your own PAT.)
