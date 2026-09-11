# Published surface admission

Next steps live in
[`PUBLISHED_SURFACE_ADMISSION_PROGRESS.md`](PUBLISHED_SURFACE_ADMISSION_PROGRESS.md) -> `## Next Steps`.

## Objective

Run the platform admission rule over the real 58-package inventory and produce a binding per-package
verdict, then execute it — before the cut turns every one of those packages into a cross-repo
negotiation.

`REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md` owns the rule, under its "Platform admission rule"
heading. It is written; it has never been applied to the inventory. Every package is admitted today by
default, because the monorepo publishes whatever carries `<IsPackable>true</IsPackable>`.

## Why this is the load-bearing decision, not the bump mechanism

Of 58 published packages, 16 are `*.Contracts`. The other 42 span framework, SaaS adapters, testing,
seeding, hosting, and AppHost packages, with their exact disposition fixed by the binding table below.

Sharing contracts across a service boundary is the intended coupling. Sharing a framework reintroduces
the compile-time coupling the service split exists to remove: five services that cannot move
independently because they all move when `Kernel` moves. The bump frequency is the readout — a shared
library whose consumers must upgrade on every producer merge is a monolith module with a version number
on it.

The current inventory already strains the plan's own stated policy:
`api/Concertable.B2B/Directory.Packages.props` consumes `Concertable.Auth.Hosting`,
`Concertable.Payment.Hosting` and `Concertable.Payment.TestKit`, and
`api/Concertable.Shared/Directory.Packages.props` pins `Concertable.Payment.Hosting` to the platform
train rather than the Payment one — the split-pin drift `api/TECH_DEBT.md` records. Whether sibling
`*.Hosting` / `*.TestKit` consumption is legitimate composition-time and test-only use, or a coupling
the boundary check should reject, is unsettled and is settled here.

## Phases

### Phase 1 — the verdict table

One row per `IsPackable` project, each with a written reason, assigning:

- **verdict** — `platform` (domain-neutral, admitted to a platform repo), `service-owned` (published,
  on its owning service or System producer train), `vendor` (unpublished; each consumer owns a copy), or `promote`
  (the capability becomes a service, not a library);
- **train** — the release train and consumer property it versions under, feeding
  `PLATFORM_RELEASE_TRAINS_PLAN.md` Phase 3;
- **post-cut owning repository**.

The rule decides admission. A second, explicit cost test decides `vendor`: when the cross-repo
coordination cost of a package exceeds the cost of duplicating it, it is vendored. Duplication inside
one deployment unit is not the same defect as duplication across one, and the seven `Shared.*` SaaS
adapter families are the obvious candidates — a thin wrapper over an external API is usually cheaper copied
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
row count equals `grep -rl '<IsPackable>true</IsPackable>' api --include='*.csproj' | wc -l` at the
reviewed head;
`python .agents/hooks/plan_graph.py --root <absolute-worktree>` green.

#### Binding verdict table

Consumer evidence includes direct `PackageReference` and in-monorepo `ProjectReference` use. The latter is
the source form of a post-cut package edge. Counts name distinct top-level product/system areas and exclude
the package's own project. `—` means that the package leaves the published surface and therefore has no
release-train property.

| Package | Consumers | Verdict | Train property | Post-cut owner | Reason |
|---|---:|---|---|---|---|
| `Concertable.AppHost.Shared` | 6 | platform | `ConcertableDotNetPlatformVersion` | `Concertable/platform-dotnet` | Domain-neutral Aspire composition used across every service and the system host. |
| `Concertable.Auth.Contracts` | 2 | service-owned | `ConcertableAuthVersion` | `Concertable/auth` | Auth-owned wire contracts consumed by B2B and Customer. |
| `Concertable.Auth.Hosting` | 6 | service-owned | `ConcertableAuthVersion` | `Concertable/auth` | Auth composition remains owner-versioned and is legitimately consumed by service and system AppHosts. |
| `Concertable.B2B.Admin.Contracts` | 0 | vendor | — | `Concertable/b2b` | No cross-repository consumer; keep the contract internal and unpublished. |
| `Concertable.B2B.Application.Contracts` | 0 | vendor | — | `Concertable/b2b` | No cross-repository consumer; keep the contract internal and unpublished. |
| `Concertable.B2B.Artist.Contracts` | 2 | service-owned | `ConcertableB2BContractsVersion` | `Concertable/b2b` | B2B-owned projection contract consumed by Customer and Search. |
| `Concertable.B2B.Booking.Contracts` | 0 | vendor | — | `Concertable/b2b` | No cross-repository consumer; keep the contract internal and unpublished. |
| `Concertable.B2B.Concert.Contracts` | 2 | service-owned | `ConcertableB2BContractsVersion` | `Concertable/b2b` | B2B-owned projection contract consumed by Customer and Search. |
| `Concertable.B2B.Deal.Contracts` | 0 | vendor | — | `Concertable/b2b` | No cross-repository consumer; keep the contract internal and unpublished. |
| `Concertable.B2B.Hosting` | 4 | service-owned | `ConcertableB2BContractsVersion` | `Concertable/b2b` | B2B composition is a service concept and remains on the B2B producer train. |
| `Concertable.B2B.Seed.Contracts` | 2 | service-owned | `ConcertableB2BContractsVersion` | `Concertable/b2b` | B2B-owned seed contract consumed by Customer and Search. |
| `Concertable.B2B.Tenant.Contracts` | 0 | vendor | — | `Concertable/b2b` | No cross-repository consumer; keep the contract internal and unpublished. |
| `Concertable.B2B.TestKit` | 1 | service-owned | `ConcertableB2BContractsVersion` | `Concertable/b2b` | Public/test-admin clients remain owner-versioned for B2B and future system tests. |
| `Concertable.B2B.User.Contracts` | 0 | vendor | — | `Concertable/b2b` | No cross-repository consumer; keep the contract internal and unpublished. |
| `Concertable.B2B.Venue.Contracts` | 2 | service-owned | `ConcertableB2BContractsVersion` | `Concertable/b2b` | B2B-owned projection contract consumed by Customer and Search. |
| `Concertable.Contracts` | 3 | platform | `ConcertableDotNetPlatformVersion` | `Concertable/platform-dotnet` | Domain-neutral result, paging and integration contract primitives have three product consumers. |
| `Concertable.Customer.Hosting` | 2 | service-owned | `ConcertableCustomerVersion` | `Concertable/customer` | Customer composition is a service concept and remains on the Customer train. |
| `Concertable.Customer.Review.Contracts` | 1 | service-owned | `ConcertableCustomerVersion` | `Concertable/customer` | Customer-owned wire contract is consumed across the B2B boundary. |
| `Concertable.Customer.TestKit` | 1 | service-owned | `ConcertableCustomerVersion` | `Concertable/customer` | Public/test-admin clients remain owner-versioned for Customer and future system tests. |
| `Concertable.Customer.Ticket.Contracts` | 1 | service-owned | `ConcertableCustomerVersion` | `Concertable/customer` | Customer-owned wire contract is consumed across the B2B boundary. |
| `Concertable.DataAccess.Application` | 4 | platform | `ConcertableDotNetPlatformVersion` | `Concertable/platform-dotnet` | Domain-neutral persistence contracts are used by four product services. |
| `Concertable.DataAccess.Infrastructure` | 5 | platform | `ConcertableDotNetPlatformVersion` | `Concertable/platform-dotnet` | Provider-neutral EF infrastructure is used by all five product services. |
| `Concertable.Frontend.Hosting` | 3 | platform | `ConcertableDotNetPlatformVersion` | `Concertable/platform-dotnet` | Domain-neutral static-frontend hosting is shared by B2B, Customer and the system host. |
| `Concertable.Grpc` | 1 | vendor | — | `Concertable/payment` | Only Payment consumes the helper; local ownership costs less than a platform release edge. |
| `Concertable.Kernel` | 5 | platform | `ConcertableDotNetPlatformVersion` | `Concertable/platform-dotnet` | Domain-neutral application primitives are used by all five product services. |
| `Concertable.Messaging.Application` | 1 | vendor | — | `Concertable/b2b` | Only B2B consumes this layer; keep it local rather than platform-versioned. |
| `Concertable.Messaging.AzureServiceBus` | 5 | platform | `ConcertableDotNetPlatformVersion` | `Concertable/platform-dotnet` | Domain-neutral transport integration is used by all five product services. |
| `Concertable.Messaging.Contracts` | 5 | platform | `ConcertableDotNetPlatformVersion` | `Concertable/platform-dotnet` | Domain-neutral message contracts are used across all product services. |
| `Concertable.Messaging.Domain` | 3 | platform | `ConcertableDotNetPlatformVersion` | `Concertable/platform-dotnet` | Domain-neutral message primitives have three product consumers. |
| `Concertable.Messaging.Infrastructure` | 5 | platform | `ConcertableDotNetPlatformVersion` | `Concertable/platform-dotnet` | Domain-neutral message persistence is used by all five product services. |
| `Concertable.Payment.Client` | 2 | service-owned | `ConcertablePaymentVersion` | `Concertable/payment` | Payment-owned client consumed by B2B and Customer. |
| `Concertable.Payment.Contracts` | 2 | service-owned | `ConcertablePaymentVersion` | `Concertable/payment` | Payment-owned wire contracts consumed by B2B and Customer. |
| `Concertable.Payment.Hosting` | 4 | service-owned | `ConcertablePaymentVersion` | `Concertable/payment` | Payment composition remains owner-versioned for service and system AppHosts. |
| `Concertable.Payment.TestKit` | 3 | service-owned | `ConcertablePaymentVersion` | `Concertable/payment` | Payment test-admin clients are legitimately consumed by B2B, Customer and system tests. |
| `Concertable.Search.Hosting` | 4 | service-owned | `ConcertableSearchVersion` | `Concertable/search` | Search composition is a service concept and remains on the Search train. |
| `Concertable.Seed.Identity` | 4 | platform | `ConcertableDotNetPlatformVersion` | `Concertable/platform-dotnet` | Domain-neutral identity seed primitives are shared across four services. |
| `Concertable.Seed.Infrastructure` | 1 | vendor | — | `Concertable/b2b` | Only B2B consumes this infrastructure; keep it local and unpublished. |
| `Concertable.Seed.Shared` | 5 | platform | `ConcertableDotNetPlatformVersion` | `Concertable/platform-dotnet` | Domain-neutral seed contracts are shared across all product services. |
| `Concertable.ServiceDefaults` | 5 | platform | `ConcertableDotNetPlatformVersion` | `Concertable/platform-dotnet` | Domain-neutral service defaults are used by all five product services. |
| `Concertable.Shared.Api` | 4 | platform | `ConcertableDotNetPlatformVersion` | `Concertable/platform-dotnet` | Domain-neutral HTTP terminals and middleware have four product consumers. |
| `Concertable.Shared.Blob.Application` | 1 | vendor | — | `Concertable/b2b` | The thin external-storage seam is cheaper to own locally than coordinate cross-repository. |
| `Concertable.Shared.Blob.Infrastructure` | 3 | vendor | — | `Concertable/auth`, `Concertable/b2b`, `Concertable/customer` | The thin external-storage adapter is cheaper to duplicate than platform-version. |
| `Concertable.Shared.Email.Application` | 3 | vendor | — | `Concertable/auth`, `Concertable/b2b`, `Concertable/customer` | The thin external-email seam is cheaper to duplicate than platform-version. |
| `Concertable.Shared.Email.Infrastructure` | 3 | vendor | — | `Concertable/auth`, `Concertable/b2b`, `Concertable/customer` | The thin external-email adapter is cheaper to duplicate than platform-version. |
| `Concertable.Shared.Geocoding.Application` | 2 | vendor | — | `Concertable/b2b`, `Concertable/customer` | The thin external-geocoding seam is cheaper to duplicate than platform-version. |
| `Concertable.Shared.Geocoding.Infrastructure` | 3 | vendor | — | `Concertable/auth`, `Concertable/b2b`, `Concertable/customer` | The thin external-geocoding adapter is cheaper to duplicate than platform-version. |
| `Concertable.Shared.Imaging.Application` | 1 | vendor | — | `Concertable/b2b` | The thin external-imaging seam is cheaper to own locally than coordinate cross-repository. |
| `Concertable.Shared.Imaging.Infrastructure` | 3 | vendor | — | `Concertable/auth`, `Concertable/b2b`, `Concertable/customer` | The thin external-imaging adapter is cheaper to duplicate than platform-version. |
| `Concertable.Shared.Notification.Infrastructure` | 2 | vendor | — | `Concertable/b2b`, `Concertable/customer` | The thin notification adapter is cheaper to duplicate than platform-version. |
| `Concertable.Shared.Pdf.Application` | 2 | vendor | — | `Concertable/b2b`, `Concertable/customer` | The thin PDF seam is cheaper to duplicate than platform-version. |
| `Concertable.Shared.Pdf.Infrastructure` | 3 | vendor | — | `Concertable/auth`, `Concertable/b2b`, `Concertable/customer` | The thin PDF adapter is cheaper to duplicate than platform-version. |
| `Concertable.Shared.QrCode.Application` | 1 | vendor | — | `Concertable/customer` | Only Customer consumes the QR-code seam; keep it local and unpublished. |
| `Concertable.Shared.QrCode.Infrastructure` | 1 | vendor | — | `Concertable/customer` | Only Customer consumes the QR-code adapter; keep it local and unpublished. |
| `Concertable.Testing` | 5 | platform | `ConcertableDotNetPlatformVersion` | `Concertable/platform-dotnet` | Domain-neutral test primitives support all five service repositories. |
| `Concertable.Testing.Architecture` | 6 | platform | `ConcertableDotNetPlatformVersion` | `Concertable/platform-dotnet` | Domain-neutral architecture assertions support all five service repositories and the system repository. |
| `Concertable.Testing.E2E` | 5 | service-owned | `ConcertableSystemVersion` | `Concertable/system` | Full-system orchestration imports service topology and therefore belongs to the system train. |
| `Concertable.Testing.Integration` | 5 | platform | `ConcertableDotNetPlatformVersion` | `Concertable/platform-dotnet` | Domain-neutral integration fixtures support all five service repositories. |
| `Concertable.Testing.Unit` | 1 | vendor | — | `Concertable/platform-dotnet` | Only DataAccess consumes the EF in-memory helper; retain it as unpublished platform test source. |

#### Named-question dispositions

1. The current inventory contains **13**, not 12, `Shared.*` adapter packages. All 13 are `vendor`:
   their external-provider seams are thin enough that consumer-local ownership costs less than a shared
   cross-repository release edge. Capability pairs move together so an Application abstraction is not left
   published after its Infrastructure adapter is vendored.
2. `Concertable.Testing`, `.Architecture`, and `.Integration` remain domain-neutral platform packages;
   `.Unit` becomes unpublished platform test source because it has one consumer. `Concertable.Testing.E2E`
   moves to the system train because it imports service topology. All three `*.TestKit` packages remain
   service-owned: they expose owner-controlled public/test-admin clients to the future system repository.
3. B2B's sibling `*.Hosting` and `Payment.TestKit` consumption is legitimate only at composition and test
   boundaries. Those packages remain on their service owner's train; production service runtime projects
   may not depend on them, and the boundary check should enforce that narrower rule when the verdicts execute.

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
