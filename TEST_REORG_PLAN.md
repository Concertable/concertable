# Test Project Reorganisation Plan

Move per-module tests into their owning modules. Cross-cutting tests stay at the
root. Drop the `InternalsVisibleTo` entries that exist only because every test
currently lives outside its module.

## Target layout

```
api/
  Modules/
    Concert/
      Concertable.Concert.UnitTests/
      Concertable.Concert.IntegrationTests/
    Venue/
      Concertable.Venue.UnitTests/
      Concertable.Venue.IntegrationTests/
    Artist/
      Concertable.Artist.UnitTests/
      Concertable.Artist.IntegrationTests/
    Auth/
      Concertable.Auth.UnitTests/
      Concertable.Auth.IntegrationTests/
    Search/
      Concertable.Search.UnitTests/
    Contract/
      Concertable.Contract.UnitTests/
      Concertable.Contract.IntegrationTests/
    Payment/
      Concertable.Payment.UnitTests/
      Concertable.Payment.IntegrationTests/
    Customer/
      Concertable.Customer.IntegrationTests/
    Messaging/
      Concertable.Messaging.IntegrationTests/
    User/
      Concertable.User.UnitTests/
  Tests/
    Concertable.Web.IntegrationTests/   (cross-module HTTP only)
    Concertable.Web.E2ETests/           (full-stack flows — STAY)
    Concertable.Workers.UnitTests/      (Workers host — STAY)
```

Per-module test projects are created on demand — only when there is content to
move into them. Empty shells are not added "for symmetry."

## Current root test inventory

### `Concertable.Web.IntegrationTests/Controllers/`

| Folder | Owning module | Destination |
|---|---|---|
| `Application/` (OpportunityApplication) | Concert | `Concert.IntegrationTests` |
| `Artist/` | Artist | `Artist.IntegrationTests` |
| `Auth/` | Auth | `Auth.IntegrationTests` |
| `Concert/` | Concert | `Concert.IntegrationTests` |
| `Opportunity/` | Concert | `Concert.IntegrationTests` |
| `Ticket/` | Concert | `Concert.IntegrationTests` |
| `Search/` | Search | stay (cross-module aggregator) **or** `Search.IntegrationTests` — decide on first read |
| `User/` | User / Identity | `User.IntegrationTests` |
| `Venue/` | Venue | `Venue.IntegrationTests` |
| `ImageFileBuilder.cs`, `ImageMappers.cs` | shared helper | move to `Tests/Concertable.Testing.Shared/` (new) or duplicate sparingly |

Anything that boots `WebApplicationFactory` and hits **multiple** module
endpoints in one test stays in `Concertable.Web.IntegrationTests`.

### `Concertable.Web.IntegrationTests/Infrastructure/`

`ApiFixture`, `SqlFixture`, `TestDbInitializer`, `TestAuthHandler`,
`MockStripeClient*`, `WebApplicationHttpClientFactory`, `IntegrationTestCollection`,
`HttpResponseAssertions`, etc. are reused by every per-module integration test.

Two options:
1. **Promote to a new `Concertable.Testing.Web/` library** (no `[Test]`s, just
   fixtures + helpers) referenced by every `*.IntegrationTests` project. Cleanest.
2. Duplicate per module. Rejected — fixture drift across modules.

Going with option 1.

### `Concertable.Core.UnitTests/`

| File | Destination |
|---|---|
| `Entities/RefreshTokenEntityTests.cs` | `Auth.UnitTests` |
| `Entities/Contracts/*` | `Contract.UnitTests` |
| `Extensions/GeoParamsExtensionsTests.cs` | wherever `GeoParams` lives now (Search? Shared?) — verify on touch |

Once empty, **delete `Concertable.Core.UnitTests`**.

### `Concertable.Infrastructure.UnitTests` / `Concertable.Infrastructure.IntegrationTests`

Audit content per file; route into the module that owns the SUT. Delete the
shells once empty. (These are explicit IVT consumers in 8 modules — biggest
cleanup payoff.)

### `Concertable.Web.E2ETests/`, `Concertable.Workers.UnitTests/`

Stay at root. Cross-cutting by definition.

## InternalsVisibleTo cleanup

Once each module owns its tests, remove the IVT entries that target a
*different* assembly than the module's own tests project. Concrete delete list
once split lands:

| Source assembly | IVT entries to drop |
|---|---|
| `Concert.Application` | `Concertable.Web.IntegrationTests`, `Concertable.Infrastructure.UnitTests`, `Concertable.Infrastructure.IntegrationTests`, `Concertable.Workers.UnitTests`, `Concertable.Web.E2ETests` |
| `Concert.Infrastructure` | same as above (minus E2E) |
| `Concert.Api` | `Concertable.Web.IntegrationTests`, `Concertable.Web.E2ETests` |
| `Contract.Application` | `Concertable.Web.IntegrationTests`, `Concertable.Infrastructure.UnitTests`, `Concertable.Infrastructure.IntegrationTests`, `Concertable.Workers.UnitTests`, `Concertable.Web.E2ETests` |
| `Contract.Infrastructure` | same minus E2E |
| `Contract.Api` | `Concertable.Web.IntegrationTests` |
| `Payment.Application` | `Concertable.Web.IntegrationTests`, `Concertable.Web.E2ETests`, `Concertable.Infrastructure.UnitTests`, `Concertable.Workers.UnitTests` |
| `Payment.Infrastructure` | `Concertable.Web.IntegrationTests`, `Concertable.Infrastructure.UnitTests` |
| `Customer.Application` | `Concertable.Web.IntegrationTests` |
| `Venue.Application` | `Concertable.Web.IntegrationTests` |
| `Artist.Application` | `Concertable.Web.IntegrationTests` |
| `Messaging.Application` | `Concertable.Web.IntegrationTests` |
| `Search.Application` | `Concertable.Core.UnitTests` |
| `Search.Infrastructure` | `Concertable.Infrastructure.UnitTests` |
| `Auth.Infrastructure` | `Concertable.Infrastructure.UnitTests` |
| `User.Infrastructure` | `Concertable.Infrastructure.UnitTests` |

Each module replaces its dropped entries with **one** entry: its own
`Module.UnitTests` / `Module.IntegrationTests` (only when those projects
actually need internal access).

`DynamicProxyGenAssembly2` entries stay where the new per-module tests still
mock internal interfaces with NSubstitute/Moq (per
`feedback_castle_proxy_ivt.md`). Audit on a per-module basis as we move tests.

`xunit.runner.visualstudio` IVT lives on each test project's own
`AssemblyInfo.cs` — copied as the new test projects are created.

## Execution order

Build must stay green at every step. One module per commit.

0. **Branch + plan committed** (this file).
1. Create `api/Tests/Concertable.Testing.Web/` shared fixture library. Migrate
   `ApiFixture`, `SqlFixture`, `TestAuthHandler`, mock Stripe clients, etc.
   `Concertable.Web.IntegrationTests` references it. **Build green.**
2. Pick one module — start with **Venue** (smallest IVT footprint, single
   `Application` IVT). Create `Concertable.Venue.IntegrationTests`, move
   `Controllers/Venue/*`, drop the `Venue.Application` IVT entry. **Build green.**
3. Repeat for **Artist**, **Auth**, **Customer**, **Messaging** (each small).
4. **Search** — confirm tests are single-module before moving; otherwise leave
   in root and just drop the Search IVT entries that point at deleted projects.
5. **Contract** — moves both unit and integration tests; collapses 6 IVT
   entries on Application + 5 on Infrastructure + 1 on Api.
6. **Concert** — biggest. `Application/` (OpportunityApplication), `Concert/`,
   `Opportunity/`, `Ticket/` controllers all collapse here. Same shape for
   Infrastructure/Api IVTs.
7. **Payment** — and audit whether any current `Web.IntegrationTests` payment
   tests are actually E2E-flavoured and belong in `Web.E2ETests` instead.
8. **User** / **Auth unit** — sweep `Core.UnitTests` contents into
   `Auth.UnitTests` + `Contract.UnitTests`; then **delete `Core.UnitTests`**.
9. Audit remaining contents of `Concertable.Infrastructure.UnitTests` and
   `Concertable.Infrastructure.IntegrationTests`. Route per file. **Delete
   the shells** once empty.
10. Final pass: every `AssemblyInfo.cs` IVT list should now contain only
    `xunit.runner.visualstudio` (on test projects), in-module sibling refs
    (`Module.Infrastructure`, `Module.Api`), and `DynamicProxyGenAssembly2`
    where mocking still requires it. Anything else gets deleted.
11. Solution file: add new test projects under
    `Modules/<Module>` solution folders (per
    `feedback_sln_solution_folder_duplicate.md` — add flat then reparent in
    VS, do not pass `--solution-folder Modules/<Module>` to `dotnet sln add`).
12. Run full test suite (`dotnet test` on solution). Fix any reflection/test
    discovery breakage caused by assembly renames.
13. Update memory: revise / supersede
    `feedback_internals_visible_to_for_tests.md` once the
    `Web.IntegrationTests`-targeted IVTs are gone.

## Decisions to confirm before starting

1. **Shared fixture library name** — `Concertable.Testing.Web` vs
   `Concertable.IntegrationTests.Shared`. Picking the former (shorter, matches
   xUnit ecosystem convention).
2. **Per-module test naming** — `Concertable.Concert.IntegrationTests` (full
   prefix) vs `Concert.IntegrationTests` (short). Picking full prefix, matches
   every other project in the solution.
3. **Where do controllers tests that span 2 modules go?** Default: stay in
   `Concertable.Web.IntegrationTests` until the cross-module surface is small
   enough to delete.
4. **`Workers.UnitTests`** — stays at root; not touched by this refactor.

## Out of scope

- Renaming or restructuring `Concertable.Web.E2ETests` (it's already correct).
- Changing test framework, fixtures, or DB strategy.
- Splitting `Web.IntegrationTests` further than per-module (e.g. by feature).
