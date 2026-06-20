# Microservice Migration — Continuation Steps

> **Picks up where** [MICROSERVICE_STEPS.md](MICROSERVICE_STEPS.md) left off (Phases 1–5 complete).
>
> **Status:** Phases 6–8 ✅ complete (2026-05-21). Phase 9 next.
>
> **Naming convention reminder:** `.Web` = ASP.NET host entry-point for a microservice (e.g. `Concertable.Search.Web`). `.Api` = module layer within the modular monolith (e.g. `Concertable.Concert.Api`). Never swap the two.

---

## Phase 6 — B2B host rename + folder restructure

> Bring B2B's folder layout in line with every other service. Zero behaviour change — this is a packaging rename only. The modular monolith inside B2B (IXModule facades, per-module DbContexts, in-process domain events, NetArchTest rules) stays exactly as-is.

19. **Rename `Concertable.Web` → `Concertable.B2B.Web`.**
    - Rename the csproj file and root namespace. Move the project into a new `api/Concertable.B2B/` folder.
    - Move `api/Modules/` → `api/Concertable.B2B/Modules/`.
    - Update `.sln` (and any `.slnx`) project paths. Update all `<ProjectReference>` paths that currently point at `../../../Modules/` — they now point at `../Modules/` (same folder, shorter path since we're inside `Concertable.B2B/`).
    - Update `Concertable.AppHost/DistributedApplicationBuilderExtensions.cs`: the `api` resource that references `Concertable.Web.csproj` becomes `b2b-web` referencing `Concertable.B2B.Web.csproj`. Service name (`concertable-b2b`) does not change — ASB topic subscriptions are keyed on it.
    - Update `Concertable.Web/Extensions/` (or wherever `IWebApplicationExtensions` lives) — namespace follows the rename.

20. **Rename `Concertable.Workers` → `Concertable.B2B.Workers`.**
    - `Concertable.Workers` is an Azure Functions Worker (minimal `Program.cs` — just `AddInfrastructure`). Rename csproj + namespace; move into `api/Concertable.B2B/`.
    - Note: `Concertable.Workers` uses Azure Functions Worker SDK (`FunctionsApplication.CreateBuilder`), while `Concertable.Search.Workers` and `Concertable.Payment.Workers` use the .NET Worker SDK (`Host.CreateApplicationBuilder`). They diverge intentionally for now — converging to one pattern is Phase 9 if it ever matters.
    - Update AppHost: `workers` resource path → `Concertable.B2B.Workers.csproj`.
    - Update `.sln` / `.slnx` project paths.

**Exit criteria:** `api/Concertable.B2B/` exists. `Concertable.B2B.Web.csproj` and `Concertable.B2B.Workers.csproj` build. All B2B modules reference compile. AppHost starts all services. No behaviour change — `dotnet run --project Concertable.AppHost` produces the same running system.

---

## Phase 7 — Dead module cleanup

> After Customer, Search, and Payment extraction, several modules remain in `Concertable.B2B/Modules/` that should not be there, or are dead code. Clean up so B2B owns exactly what the architecture says it should.

21. ✅ **Move `Modules/Search/` → `Concertable.Search/`** (2026-05-21)
    Search module layers (Api, Application, Domain, Infrastructure, Tests) were still inside `Concertable.B2B/Modules/Search/` even though Search is a separate service. Moved all four project folders and `Tests/` directly under `Concertable.Search/`, matching the Payment layout. Updated csproj ProjectReference paths in all six module projects, Search.Web.csproj, Search.Workers.csproj, and six .sln entries. Deleted the now-empty `Concertable.B2B/Modules/Search/` folder.

22. **`Modules/Customer/` — deferred: not dead code.**
    Audit found this is a B2B-side user preferences module (location radius + genre preferences for concert notification targeting). `ConcertService.PostAsync` calls `ICustomerModule.GetUserIdsByLocationAndGenresAsync` to find users to notify on concert post. Removing it requires moving the preference data to `Concertable.Customer/` and replacing the in-process call with a gRPC/HTTP cross-service call (or an event-driven projection). That is a full architectural step, not a cleanup. Deferred.

23. ✅ **Deleted `Modules/Messaging/`** (2026-05-21)
    Folder contained only NuGet restore artifacts (obj/ directories) — zero source files, zero .csproj files. `Modules/Conversations/` is the active in-app messaging module (518 files, registered via `AddConversationsApi()`). Messaging was an abandoned predecessor stub.

**Exit criteria:** B2B modules folder contains only modules B2B owns per §2 of the architecture doc: Artist, Authorization, Concert, Contract, Conversations, User, Venue, Notification. No Search, no Customer, no dead stubs. Build green.

---

## Phase 8 — Notification module: clarify scope

> The architecture doc says Email/Notification becomes `Concertable.Email` (shared library). That refers to *email sending* (`IEmailSender`). The `Modules/Notification/` module also provides the **SignalR hub** (`NotificationHub`), which is a different concern. Separate the two cleanly.

24. ✅ **Separate SignalR notifications from email in `Modules/Notification/`.** (2026-05-21)
    - `AddNotificationModule()` was already purely SignalR — no `IEmailSender` overlap. Email separation was already clean.
    - Completed the `IXNotifier` wrapper pattern: `IConversationsNotifier` / `ConversationsNotifier` added in `Modules/Conversations/` (the only module still injecting `INotificationModule` directly). `IConcertNotifier` and `ITicketNotifier` were already in place from Step 15.
    - `MessageService` now injects `IConversationsNotifier` instead of `INotificationModule` directly.
    - Fixed pre-existing duplicate `global using Concertable.DataAccess.Infrastructure` in Conversations.Infrastructure GlobalUsings.

**Exit criteria met:** `AddSharedEmail()` and `AddNotificationModule()` have non-overlapping responsibilities. All modules reach `INotificationModule` through a typed `IXNotifier` wrapper — no direct injection of `INotificationModule` outside the notifier layer.

---

## Phase 9 — Tenancy + user-model refactor (B2B internal)

> Tenant-scoping (the Tenant module, request-scoped filtering, the compliance value object) has shipped. The remaining User/Membership/role separation inside B2B is tracked in `USER_MODEL_PLAN.md`. This is pure B2B-internal work — no cross-service impact.

25. **Execute `USER_MODEL_PLAN.md`.**
    Read that doc for exact steps. It's B2B-only modular monolith work — the microservices boundary doesn't affect it. Do this after Phases 6–8 so the folder structure is stable first.

---

## Phase 10 — B2B feature work

> Feature gaps that were parked during the microservices migration.

26. **Unpark `Feature/ManagerFrontPage`.**
    Branch parked at `23c8fc4c` (PR #50 draft). The `ConcertEntity` decomposition (Phase 1 Step 1) rewrote the data shape underneath the dashboard — verify that the parked work still compiles and aligns with the post-decomposition entity shape before continuing.

---

## What's NOT in these steps

- **Event schema versioning** (was Phase 5 Step 17) — skipped. No live users; nuke DB and re-scaffold if an event shape changes.
- **Notification extraction to a service** (was Phase 5 Step 18) — skipped. Email stays as shared library; SignalR hub stays in B2B. Extract only if operational pressure demands it.
- **Workers host unification** (Azure Functions vs Worker SDK) — deferred to Phase 9 if it ever creates friction. Both patterns work; the divergence is cosmetic for now.
- **RabbitMQ transport** — the architecture §9 listed this as a learning step between in-memory and ASB. Skipped — ASB is already running in dev via the Aspire emulator. No value in adding an intermediate broker.
