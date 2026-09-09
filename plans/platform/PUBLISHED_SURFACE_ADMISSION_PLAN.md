# Published surface admission

Next steps live in
[`PUBLISHED_SURFACE_ADMISSION_PROGRESS.md`](PUBLISHED_SURFACE_ADMISSION_PROGRESS.md) -> `## Next Steps`.

## Objective

Run the platform admission rule over the real 55-package inventory and produce a binding per-package
verdict, then execute it — before the cut turns every one of those packages into a cross-repo
negotiation.

`REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md` states the rule: a package belongs in a platform repo
only when it is domain-neutral, has at least two legitimate product consumers, and can version without
importing a service concept. The rule is written; it has never been applied to the inventory. Every
package is admitted today by default, because the monorepo publishes whatever carries
`<IsPackable>true</IsPackable>`.

## Why this is the load-bearing decision, not the bump mechanism

Of 55 published packages, roughly 13 are `*.Contracts`. The other 42 are a shared framework: `Kernel`,
`DataAccess` (2), `Messaging` (5), `ServiceDefaults`, `Grpc`, `Shared.Api`, seven `Shared.*` SaaS
adapters split across Application and Infrastructure (12), five `Testing.*`, three `*.TestKit`, three
`Seed.*`, five `*.Hosting`, `AppHost.Shared`.

Sharing contracts across a service boundary is the intended coupling. Sharing a framework reintroduces
the compile-time coupling the service split exists to remove: five services that cannot move
independently because they all move when `Kernel` moves. The bump frequency is the readout — a shared
library whose consumers must upgrade on every producer merge is a monolith module with a version number
on it.

The current inventory already exceeds the plan's own stated policy in at least one place:
`api/Concertable.B2B/Directory.Packages.props` consumes `Concertable.Auth.Hosting`,
`Concertable.Payment.Hosting` and `Concertable.Payment.TestKit`. Whether that is legitimate
composition-time and test-only use under the `*.Hosting` / `*.TestKit` policy, or a coupling the
boundary check should reject, is unsettled and is settled here.

## Phases

### Phase 1 — the verdict table

One row per `IsPackable` project, each with a written reason, assigning:

- **verdict** — `platform` (domain-neutral, admitted to a platform repo), `service-owned` (published,
  but on its owning service's train), `vendor` (unpublished; each consumer owns a copy), or `promote`
  (the capability becomes a service, not a library);
- **train** — the release train and consumer property it versions under, feeding
  `PLATFORM_RELEASE_TRAINS_PLAN.md` Phase 3;
- **post-cut owning repository**.

The rule decides admission. A second, explicit cost test decides `vendor`: when the cross-repo
coordination cost of a package exceeds the cost of duplicating it, it is vendored. Duplication inside
one deployment unit is not the same defect as duplication across one, and the seven `Shared.*` SaaS
adapters are the obvious candidates — a thin wrapper over an external API is usually cheaper copied
than negotiated across nine repos.

Three questions the table must answer explicitly rather than by omission:

1. The 12 `Shared.*` adapter packages — admitted, vendored, or promoted to services.
2. The five `Testing.*` plus three `*.TestKit` packages — whether test infrastructure crossing a service
   boundary is legitimate, or an artifact of a monorepo-shaped E2E strategy that the cut will strand.
3. B2B's consumption of sibling `*.Hosting` and `*.TestKit` — legitimate under the policy, or a
   violation the boundary check must start rejecting.

**Consumption contract** — the table is consumed by `PLATFORM_RELEASE_TRAINS_PLAN.md` Phase 3 for train
membership, by the cut's Stage 3 and Stage 4 streams for repository assignment, and by the Renovate
preset for producer-train grouping. It must therefore name, per package, its verdict, its train
property, and its post-cut owning repository — not a verdict alone.

**Verification gate** — every `IsPackable` project appears exactly once with a verdict and a reason; the
generated row count equals `grep -rl '<IsPackable>true</IsPackable>' api --include='*.csproj' | wc -l`;
`python .agents/hooks/plan_graph.py --root <absolute-worktree>` green.

### Phase 2 onward — execute the verdicts

Each verdict class spins off its own plan rather than being forced into this one, because they have
unrelated blast radii and gates: a vendoring batch deletes packages and duplicates source, a promotion
turns a library into a deployable with its own database and events, and a boundary-check tightening
changes what CI rejects. This plan stays open until every verdict is executed or explicitly deferred
with a recorded reason.

## Relationship to the cut

Nothing here blocks the cut and the cut does not block this. The cut can carry the current surface
across; it will simply carry the coupling with it, and every verdict deferred past the cut is executed
afterwards across N repositories instead of once inside one.
