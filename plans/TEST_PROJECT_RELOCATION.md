# Test-project relocation — dissolve `api/Tests/`, push ownership into services

Relocate every test project to the owner dictated by `ARCHITECTURE.md`: service-owned test
code lives under `api/Concertable.<Service>/`, genuinely cross-service "shared kernel" test
infra lives under `api/Shared/Tests/`. Delete orphan stubs and dead dirs along the way.

**Full sweep (chosen):** execute ALL of Slices 1–6 — evict every service-owned project from
`api/Tests/` (Slices 1–4), purge the Payment dependency from the shared kernel test infra
(Decision C), move the remaining genuinely-shared infra to `api/Shared/Tests/` (Slice 5), and
**delete `api/Tests/` entirely** (Slice 6). `api/Tests/` should not exist when done. (Decision D
permitted leaving a shared remainder behind, but the user opted for the complete dissolution.)

This is a **self-contained plan**. Execute it cold after `/clear`. It states the verified
current layout, the target, the open decisions (answer before/while executing), the exact
file/path edits, the per-service slice ordering, and verification.

> Why this exists: `api/Tests/` mixes shared-kernel infra with B2B/Customer/Search-owned test
> projects, and a prior half-finished move left an **orphan stub** and **empty dead dirs**.
> Per `ARCHITECTURE.md`: "Anything beyond Contracts — Domain, Application, Infrastructure,
> Seeding — stays private to the owning service." Tests are owned code; they belong to the
> service, not a shared bucket.

## 0. Verified current state (read before trusting anything below)

### 0.1 Solution files (ignore `.vs/` copies)
- `api/Concertable.slnx` — umbrella (references everything)
- `api/Concertable.B2B/Concertable.B2B.slnx`
- `api/Concertable.Customer/Concertable.Customer.slnx`
- `api/Concertable.Search/Concertable.Search.slnx`
- `api/Concertable.Payment/Concertable.Payment.slnx`

### 0.2 What lives in `api/Tests/` today

| Project / dir | Source? | Owner | Verdict |
|---|---|---|---|
| `Concertable.Testing` | real (3 .cs) | **shared kernel** | → `api/Shared/Tests/` |
| `Concertable.Testing.Integration` | real (10 .cs) | **shared kernel** (refs Payment.Client, Email/Geocoding/Imaging.Application as shared mocks) | → `api/Shared/Tests/` (see Decision C) |
| `Concertable.E2ETests` | real (36 .cs) | **system E2E harness** (NOT shared kernel — boots the whole distributed app via `DistributedApplicationBuilderExtensions`; system-aware by nature) | → `api/Shared/Tests/` after extracting Payment-domain helpers (Slice 4b) and severing the `Payment.Infrastructure` string-dep (§0.7). Keeps name `Concertable.E2ETests` (do NOT rename `.Kernel` — it isn't agnostic). |
| ↳ `Concertable.E2ETests.Api` (subdir) | **empty dead dir** | — | **DELETE** (Slice 1) |
| ↳ `Concertable.E2ETests.Mobile` (subdir) | real Appium suite | cross-service test suite | **Deferred** — relocation is long-way-off debt (Decision B). Sweep only repoints its ref to the moved harness. |
| ↳ Payment/Stripe helpers inside `Concertable.E2ETests` (`StripeFixture`, `StripeCards`, `PaymentDb`, `PaymentDbFixture`, `Support/StripePayment*`, `Support/IStripePayment`, `Support/StripeCardEntry`) | real | **Payment** | → new Payment-owned E2E support lib (Slice 4b) |
| `Concertable.Testing.Integration.B2B` | real (23 .cs) | **B2B** | → `api/Concertable.B2B/Tests/` |
| `Concertable.Testing.Integration.Customer` | real (1 .cs = `ApiFixture`) | **Customer** | → `api/Concertable.Customer/Tests/` |
| `Concertable.Testing.Integration.Search` | real (2 .cs) | **Search** | → `api/Concertable.Search/Tests/` |
| `Concertable.Workers.UnitTests` | real (1 .cs) | **B2B** (refs B2B.Workers, B2B.Concert.*) | → `api/Concertable.B2B/Tests/` |
| `Concertable.Customer.E2ETests.Ui` | **STUB** (csproj + bin/obj + `ui-tests.last.log`, 0 source) | — | **DELETE** (orphan; real copy already under Customer) |
| `Concertable.IntegrationTests.Common` | **empty dir** | — | **DELETE** |
| `Concertable.Tests.Common` | **empty dir** | — | **DELETE** |
| `Directory.Build.targets` + `FilterNdjsonTagsTask.cs` + `StripFeatureTraitsTask.cs` + `StripReqnrollHooksTask.cs` | MSBuild infra | shared | → `api/Shared/Tests/` (Reqnroll trait-stripping; see §0.5) |
| `TESTS.md`, `VS_TEST_EXPLORER_TROUBLESHOOTING.md` | docs | shared | → `api/Shared/Tests/` or `api/docs/` (Decision D) |

### 0.3 Orphan-stub confirmation
`api/Concertable.slnx:205` references the **real** Customer UI project
(`Concertable.Customer/Tests/E2ETests/Concertable.Customer.E2ETests.Ui/...`). The stub at
`api/Tests/Concertable.Customer.E2ETests.Ui` is referenced by **no solution** → deleting it
breaks nothing. (Also delete the empty leftover `api/Concertable.Customer/Concertable.Customer.Seeding/`.)

### 0.4 Umbrella `Concertable.slnx` `/Tests/` folder (lines ~208–215) — the entries to rewrite/remove
```
Tests/Concertable.E2ETests/Concertable.E2ETests.csproj
Tests/Concertable.E2ETests/Concertable.E2ETests.Mobile/Concertable.E2ETests.Mobile.csproj
Tests/Concertable.Testing/Concertable.Testing.csproj
Tests/Concertable.Testing.Integration/Concertable.Testing.Integration.csproj
Tests/Concertable.Testing.Integration.B2B/Concertable.Testing.Integration.B2B.csproj
Tests/Concertable.Testing.Integration.Customer/Concertable.Testing.Integration.Customer.csproj
Tests/Concertable.Testing.Integration.Search/Concertable.Testing.Integration.Search.csproj
Tests/Concertable.Workers.UnitTests/Concertable.Workers.UnitTests.csproj
```
(Note `api/Shared/Tests/Concertable.Kernel.UnitTests` already exists in the umbrella at line 197 —
so `api/Shared/Tests/` is an established home for shared-kernel test projects.)

### 0.5 MSBuild build-infra status (load-bearing — do not skip)
- The Reqnroll trait-stripping `Directory.Build.targets` has **already been duplicated** into
  `api/Concertable.B2B/Tests/E2ETests/Directory.Build.targets` and
  `api/Concertable.Customer/Tests/E2ETests/Directory.Build.targets`, so the **already-relocated**
  UI E2E projects keep their build behaviour.
- The only Reqnroll project still relying on the **top-level** `api/Tests/Directory.Build.targets`
  is `Concertable.E2ETests.Mobile` (Appium, has `.feature` files). When `api/Tests/` is dissolved,
  Mobile must land somewhere that still imports an equivalent `Directory.Build.targets`.
- **Verify during execution:** that the B2B/Customer copies reference co-located task `.cs`
  files (`$(MSBuildThisFileDirectory)...Task.cs`) and don't path back into `api/Tests/`. If they
  point back, fix to the new shared location.

### 0.6 Who references the moving projects (ProjectReference paths to rewrite)
- **`Concertable.Testing.Integration.B2B`** ← the 7 B2B module test csprojs
  (`Modules/{Artist,Concert,Contract,Organization,User,Venue}/Tests/*`).
- **`Concertable.Testing.Integration.Customer`** ← 5 Customer module test csprojs
  (`Modules/{Concert,Review,Ticket,User}/Tests/*`).
- **`Concertable.Testing.Integration.Search`** ← `Concertable.Search.IntegrationTests`.
- **`Concertable.Testing`** ← ~20 projects across all services (base utils).
- **`Concertable.Testing.Integration`** ← the three `.Integration.{B2B,Customer,Search}` fixtures.
- **`Concertable.E2ETests`** ← `Concertable.B2B.E2ETests`, `Concertable.Customer.E2ETests`,
  `Concertable.E2ETests.Mobile`.
- **`Concertable.Workers.UnitTests`** ← nothing (leaf).

Every `<ProjectReference>` to a moved project, in every consuming csproj, must be repointed.
Grep is the source of truth at execution time (see §6).

### 0.7 Payment-domain E2E helpers (Slice 4b) — facts
- The base `Concertable.E2ETests` references `Concertable.Payment.Infrastructure` for ONE thing:
  `using PaymentSchema = Concertable.Payment.Infrastructure.Schema;` → `Schema.Name` (`"payment"`)
  and `Schema.PayoutAccounts` (`"PayoutAccounts"`) in `PaymentDbFixture` Respawn ignore-list. The
  sibling `PaymentDb.cs` already hardcodes `"payment.PayoutAccounts"` in raw SQL. Trivially severed.
- The Payment-domain helpers and their consumers (all to be repointed to the new Payment E2E lib):
  - `StripeFixture` ← `Concertable.B2B.E2ETests/AppFixture`
  - `StripeCards` ← `B2B.E2ETests.Ui` (Artist/VenueManager steps), `Customer.E2ETests.Ui` (Customer steps), `Mobile` (Customer steps)
  - `PaymentDb` / `PaymentDbFixture` ← `B2B.E2ETests/DbFixture`, `Customer.E2ETests/DbFixture`
  - `StripePayment` / `IStripePayment` / `StripeCardEntry` ← `B2B.E2ETests.Ui` + `Customer.E2ETests.Ui` (ScenarioDependencies, page objects, steps)
  - (`PaymentDb` is also named in `AppHost.Shared/Constants.cs`, B2B/Customer/Payment AppHost `Program.cs`, Payment runtime, and `Testing.Integration.B2B/ApiFixture` — but those are the **DB name constant**, not this fixture type. Don't confuse them; only the test-fixture usages move.)
- The Stripe UI helpers use Playwright; the new Payment E2E lib needs the `Microsoft.Playwright`
  package. `PaymentDbFixture` uses `RespawnableDb` + `AppHostConstants` from the harness, so the new
  Payment E2E lib references the moved `Concertable.E2ETests` harness (service E2E lib → system harness; fine).

## 1. Target layout

```
api/Shared/Tests/
  Concertable.Testing/                         (moved from api/Tests/)
  Concertable.Testing.Integration/             (moved; shared SqlFixture/TestAuthHandler/Mocks)
  Concertable.E2ETests/                         (moved; system E2E harness, Payment-free after 4b)  ┐ Decision B
    Concertable.E2ETests.Mobile/                (rides along; relocation deferred)                 ┘
    (Concertable.E2ETests.Api/ — EMPTY dead dir, deleted in Slice 1, NOT moved)
  Directory.Build.targets + *Task.cs            (moved; shared Reqnroll build infra)
  Concertable.Kernel.UnitTests/                 (already here)

api/Concertable.B2B/Tests/
  Concertable.B2B.IntegrationTests.Fixtures/    (was Concertable.Testing.Integration.B2B — Decision A)
  Concertable.B2B.Workers.UnitTests/            (was Concertable.Workers.UnitTests — Decision A)
  E2ETests/...                                  (already here)

api/Concertable.Customer/Tests/
  Concertable.Customer.IntegrationTests.Fixtures/  (was Concertable.Testing.Integration.Customer — Decision A; now owns MockCustomerPaymentClient — Decision C)
  E2ETests/...                                  (already here)

api/Concertable.Search/Tests/
  Concertable.Search.IntegrationTests.Fixtures/ (was Concertable.Testing.Integration.Search — Decision A)
  Concertable.Search.IntegrationTests/          (already here)
  Concertable.Search.UnitTests/                 (already here)

api/Concertable.Payment/Tests/
  E2ETests/Concertable.Payment.E2ETests.Helpers/  (NEW — Stripe/PaymentDb E2E helper lib, Slice 4b; bare `.E2ETests` reserved for a future real suite)
  Concertable.Payment.UnitTests/                (already here)

api/Tests/   ← DELETED ENTIRELY
```

## 2. Open decisions (answer before/while executing — do not guess)

**A. Rename the relocated fixture projects, or relocate-only?**
The names `Concertable.Testing.Integration.{B2B,Customer,Search}` carry a `Testing` prefix that
implies shared-kernel infra — misleading once they live inside a single service.
- **(recommended) Rename** to service-owned names, e.g.
  `Concertable.B2B.IntegrationTests.Fixtures`, `Concertable.Customer.IntegrationTests.Fixtures`,
  `Concertable.Search.IntegrationTests.Fixtures`; `Concertable.Workers.UnitTests` →
  `Concertable.B2B.Workers.UnitTests`. Requires renaming root namespaces + every `using` +
  every `<ProjectReference>`/`<Using>` Include. Highest churn, cleanest result.
- **(fallback) Relocate only** — keep the names, just change folder + solution paths. Lower
  churn, but leaves the misleading `Testing.Integration.X` naming inside a service.
Decision: **RENAME** (chosen). Targets:
`Concertable.Testing.Integration.B2B` → `Concertable.B2B.IntegrationTests.Fixtures`;
`Concertable.Testing.Integration.Customer` → `Concertable.Customer.IntegrationTests.Fixtures`;
`Concertable.Testing.Integration.Search` → `Concertable.Search.IntegrationTests.Fixtures`;
`Concertable.Workers.UnitTests` → `Concertable.B2B.Workers.UnitTests`. Rename root namespace +
all `using`s + every `<ProjectReference>`/`<Using>` Include accordingly.

**B. Where does `Concertable.E2ETests.Mobile` live?** It's cross-service (Appium tests that drive
B2B + Customer flows) and references both `Concertable.E2ETests` and `Concertable.Customer.E2ETests`.
- Keep with the shared E2E infra under `api/Shared/Tests/Concertable.E2ETests/`.
- Or relocate under Customer (if it's really customer-journey mobile). Needs the Reqnroll
  `Directory.Build.targets` at its new home (§0.5).
Decision: **Do NOT group Mobile with the shared E2E kernel.** It's a test *suite* (references
`Customer.E2ETests`), not infra. Relocating it properly (likely under Customer) is **long-way-off
tech debt, explicitly NOT addressed in this sweep** — leave its internals untouched. For the sweep
it only needs its `<ProjectReference>` to the renamed E2E kernel repointed so it keeps building;
its physical home is decided later. (Interim location TBD — see note in Slice 5.)

**C. `Concertable.Testing.Integration` referencing Payment.Client / Email / Geocoding / Imaging.**
These are shared mock targets (adapter-service clients + cross-cutting app abstractions). Treat as
an **acceptable shared-mock surface** (keep as-is, document in its `CLAUDE.md`) or a leak to break?
Recommended: keep — they're the shared mocks every service's integration tests reuse. Confirm.

> Verified facts:
> - `Concertable.Payment.Client` is **Payment-owned** (under `api/Concertable.Payment/`, pulls
>   `Concertable.Payment.Domain`) — NOT shared kernel. The other refs
>   (`Concertable.Shared.Email/Geocoding/Imaging.Application`, `Concertable.Messaging.Infrastructure`)
>   ARE shared kernel.
> - `MockCustomerPaymentClient` (impl of `ICustomerPaymentClient`) is a fixed stub used IDENTICALLY.
>   Its **only real production consumer is Customer** (`Customer.Ticket.Infrastructure/TicketService`).
> - **B2B production code never resolves `ICustomerPaymentClient`** (grep: zero non-test hits). The
>   B2B fixture's `services.AddScoped<ICustomerPaymentClient, MockCustomerPaymentClient>()`
>   (`Tests/Concertable.Testing.Integration.B2B/ApiFixture.cs:105`) is **DEAD registration** —
>   cargo-culted, registers a mock for an interface B2B never injects. B2B's real Payment usage is
>   `IManagerPaymentClient` (+ `IEscrowClient`), mocked by B2B's own `MockManagerPaymentClient` /
>   `MockEscrowClient` already living in `Tests/Concertable.Testing.Integration.B2B/Mocks/`.
> - Transitive Payment.Client availability survives the purge: B2B fixture gets it via
>   `Concertable.Payment.Infrastructure`; Customer fixture gets it via `Concertable.Customer.Web`.

Decision: **Move `MockCustomerPaymentClient` to Customer; purge Payment from shared. No
duplication** (chosen — `ICustomerPaymentClient` "fairly obviously belongs in Customer").
- Move `Mocks/MockCustomerPaymentClient.cs` from `Concertable.Testing.Integration` →
  `Concertable.Customer.IntegrationTests.Fixtures` (add an explicit `Concertable.Payment.Client`
  `<ProjectReference>` there for honesty; it's also available transitively via `Customer.Web`).
- **Delete the dead line** `ApiFixture.cs:105` in the B2B fixture (`AddScoped<ICustomerPaymentClient,
  MockCustomerPaymentClient>()`). B2B keeps its real `MockManagerPaymentClient` / `MockEscrowClient`
  unchanged (they compile via the existing transitive `Payment.Client` from `Payment.Infrastructure`).
  **No B2B Payment mock duplication needed** — B2B never used the Customer one.
- Remove `Mocks/MockCustomerPaymentClient.cs` AND the `Concertable.Payment.Client`
  `<ProjectReference>` from `Concertable.Testing.Integration`. Shared kernel test infra then
  references ONLY `Concertable.Shared.*` + `Concertable.Messaging.Infrastructure`.
- Update `Concertable.Testing.Integration/CLAUDE.md`: remove `MockCustomerPaymentClient` from the
  "What belongs here" list (it's now Customer-owned).

**D. Folder nesting + docs home.** Inside each service `Tests/`, nest fixtures under
`Tests/Fixtures/` or keep flat `Tests/<project>/`? And do `TESTS.md` /
`VS_TEST_EXPLORER_TROUBLESHOOTING.md` go to `api/Shared/Tests/` or `api/docs/`?
Decision: **Flat `Tests/<project>/`** (no `Fixtures/` tier), **docs → `api/docs/`**. Genuinely-
shared test infra MAY remain in `api/Tests/` if it doesn't move cleanly — dissolving `api/Tests/`
entirely is a nice-to-have, NOT a hard requirement. The hard requirement is evicting service-owned
projects (Slices 1–4) and the Payment purge (Decision C). Moving the remaining shared infra to
`api/Shared/Tests/` (Slice 5) is optional polish for consistency with `Concertable.Kernel.UnitTests`.

**E. Scope/ordering vs `CUSTOMER_SEEDSTATE.md`.** This plan relocates the Customer integration
fixture that `CUSTOMER_SEEDSTATE.md` §6 step 6 edits.
Decision: **Customer slice first**, then resume `CUSTOMER_SEEDSTATE.md` against the fixture's new
home (`api/Concertable.Customer/Tests/Concertable.Customer.IntegrationTests.Fixtures/`). The
`CUSTOMER_SEEDSTATE.md` plan already exists at `plans/CUSTOMER_SEEDSTATE.md` and will be tackled
in a separate session/context — a cross-reference note has been added there.

## 3. Implementation order — per-service slices, each independently green

Do one slice at a time; build after each so breakage is localized. `git mv` to preserve history.

### Slice 1 — Delete dead weight (zero-risk, do first)
1. Delete orphan stub dir `api/Tests/Concertable.Customer.E2ETests.Ui/` (referenced by no solution — §0.3).
2. Delete empty dirs `api/Tests/Concertable.IntegrationTests.Common/`, `api/Tests/Concertable.Tests.Common/`,
   and `api/Tests/Concertable.E2ETests/Concertable.E2ETests.Api/` (empty — no csproj, no files).
3. Delete empty leftover `api/Concertable.Customer/Concertable.Customer.Seeding/`.
4. No solution edits needed (none reference these). Build umbrella — still green.

### Slice 2 — Search (smallest service-owned move)
1. `git mv api/Tests/Concertable.Testing.Integration.Search` → `api/Concertable.Search/Tests/<name per Decision A>`.
2. If renaming (Decision A): rename csproj, root namespace, `using`s.
3. Repoint `<ProjectReference>` in `Concertable.Search.IntegrationTests.csproj` to the new path/name.
4. Remove the entry from umbrella `Concertable.slnx` `/Tests/` folder; add under Search folder.
   Add to `Concertable.Search.slnx`.
5. Build `api/Concertable.Search/Concertable.Search.slnx` + umbrella → green.

### Slice 3 — Customer (+ Payment-mock move, Decision C)
1. `git mv api/Tests/Concertable.Testing.Integration.Customer` →
   `api/Concertable.Customer/Tests/Concertable.Customer.IntegrationTests.Fixtures`.
2. Rename (Decision A): csproj → `Concertable.Customer.IntegrationTests.Fixtures`, root namespace,
   `using`s.
3. **Payment-mock move (Decision C):** move `Mocks/MockCustomerPaymentClient.cs` from
   `Concertable.Testing.Integration` into this fixture, and add an explicit `Concertable.Payment.Client`
   `<ProjectReference>` HERE (also available transitively via `Customer.Web`). Customer's `ApiFixture`
   already uses it (`AddScoped<ICustomerPaymentClient, MockCustomerPaymentClient>()`) — no usage change.
4. Repoint `<ProjectReference>` in the 5 Customer module test csprojs (§0.6) to the new path/name.
5. Umbrella `Concertable.slnx`: remove old `/Tests/` entry; add under the existing `/Customer/...`
   area. Add to `Concertable.Customer.slnx`.
6. Build `Concertable.Customer.slnx` + umbrella → green.
7. **Hand-off:** `CUSTOMER_SEEDSTATE.md` can now proceed against the fixture's new location.

### Slice 4 — B2B (two projects + delete dead Payment registration, Decision C)
1. `git mv api/Tests/Concertable.Testing.Integration.B2B` →
   `api/Concertable.B2B/Tests/Concertable.B2B.IntegrationTests.Fixtures`. (Carries its own
   `Mocks/` — `MockManagerPaymentClient`, `MockEscrowClient`, Stripe/webhook mocks — unchanged.)
2. `git mv api/Tests/Concertable.Workers.UnitTests` →
   `api/Concertable.B2B/Tests/Concertable.B2B.Workers.UnitTests`.
3. Rename (Decision A): csprojs, root namespaces, `using`s.
4. **Delete dead registration (Decision C):** remove the line
   `services.AddScoped<ICustomerPaymentClient, MockCustomerPaymentClient>();` from the B2B
   `ApiFixture` (was line 105) — B2B production never resolves `ICustomerPaymentClient`. Leave
   `MockManagerPaymentClient` / `MockEscrowClient` untouched (they compile via the existing
   transitive `Payment.Client` from `Payment.Infrastructure`). **No duplication** — B2B never used
   the Customer mock.
5. Repoint `<ProjectReference>` in the 7 B2B module test csprojs (§0.6).
6. Umbrella: remove both old `/Tests/` entries; add under B2B. Add to `Concertable.B2B.slnx`.
7. Build `Concertable.B2B.slnx` + umbrella → green.

### Slice 4b — Extract Payment-domain E2E helpers into a Payment-owned lib (§0.7)
> Name it `Concertable.Payment.E2ETests.Helpers` (a HELPER lib, not a test suite — it holds
> fixtures/helpers consumed by other services). Reserve the bare `Concertable.Payment.E2ETests`
> name for a genuine future Payment E2E suite (which could itself reference `.Helpers`). Not
> `.Shared`/kernel — it's Payment-domain (Stripe + Payment schema), just Payment-owned.
1. Create `api/Concertable.Payment/Tests/E2ETests/Concertable.Payment.E2ETests.Helpers/` (new
   project; add `Microsoft.Playwright` for the Stripe UI helpers). It references the system harness
   `Concertable.E2ETests` (still in `api/Tests/` at this point — repointed in Slice 5).
2. `git mv` into it the Payment-domain helpers from `Concertable.E2ETests`:
   `StripeFixture.cs`, `StripeCards.cs`, `PaymentDb.cs`, `PaymentDbFixture.cs`,
   `Support/StripePayment.cs`, `Support/IStripePayment.cs`, `Support/StripeCardEntry.cs`.
3. **Sever the Payment.Infrastructure string-dep** while moving `PaymentDbFixture`: replace
   `PaymentSchema.Name` / `PaymentSchema.PayoutAccounts` with the literals `"payment"` /
   `"PayoutAccounts"` (matching what `PaymentDb.cs` already hardcodes). The base
   `Concertable.E2ETests` then drops its `Concertable.Payment.Infrastructure` `<ProjectReference>`
   → harness becomes Payment-free.
4. Repoint the consumers (§0.7) to the new lib: `B2B.E2ETests`, `B2B.E2ETests.Ui`,
   `Customer.E2ETests`, `Customer.E2ETests.Ui`, `Mobile` — add a `<ProjectReference>` to
   `Concertable.Payment.E2ETests` and fix `using`s/`<Using>` Includes for the moved namespaces.
5. Add the new project to `Concertable.Payment.slnx` + umbrella `Concertable.slnx`.
6. Build `Concertable.Payment.slnx` + umbrella → green.

### Slice 5 — System E2E harness + shared kernel → `api/Shared/Tests/`
0. **Payment purge finish (Decision C):** delete `Mocks/MockCustomerPaymentClient.cs` and the
   `Concertable.Payment.Client` `<ProjectReference>` from `Concertable.Testing.Integration` (now
   unused after Slices 3+4), and remove `MockCustomerPaymentClient` from the "What belongs here"
   list in `Concertable.Testing.Integration/CLAUDE.md`. Shared project then references ONLY
   `Concertable.Shared.*` + `Concertable.Messaging.Infrastructure`.
1. `git mv` `Concertable.Testing`, `Concertable.Testing.Integration`, and `Concertable.E2ETests`
   (now Payment-free per Slice 4b; **keep name — NOT renamed `.Kernel`**; carries the deferred
   `Concertable.E2ETests.Mobile` subdir along for now per Decision B), plus the MSBuild infra
   (`Directory.Build.targets` + the three `*Task.cs`) + docs → `api/docs/` (Decision D), into
   `api/Shared/Tests/`.
2. Repoint EVERY consuming `<ProjectReference>` (the ~20 base-utils refs; the E2E suites' + Payment
   E2E lib's ref to `Concertable.E2ETests`; the three fixtures' ref to `Concertable.Testing.Integration`).
   Grep-driven (§6).
3. Verify §0.5: relocated `Mobile` (and any other Reqnroll project) imports a `Directory.Build.targets`;
   task `.cs` paths resolve.
4. Umbrella `Concertable.slnx`: rewrite the `/Tests/` folder entries to the `Shared/Tests/...` paths.
5. Build umbrella → green.

### Slice 6 — Dissolve `api/Tests/`
1. Confirm `api/Tests/` is now empty of projects (only stragglers left should be none).
2. Delete the `api/Tests/` directory.
3. Global grep for any lingering `Tests/Concertable.` or `..\Tests\` / `../Tests/` path
   fragments in any `.csproj`/`.slnx`/`.targets`/`.props` and fix.
4. Build umbrella + all per-service solutions → green.

## 4. Mechanics / gotchas
- **`git mv`** each project dir to preserve blame/history; commit per slice.
- **Delete `bin/`+`obj/`** in every moved project before/after to avoid stale build artifacts
  resurrecting old paths (the `reset-test-explorer` skill exists for VS stale-trait issues).
- **`.slnx` paths are forward-slash, relative to the solution file.** Umbrella paths are
  repo-root-relative (`Concertable.X/...`); per-service `.slnx` paths are service-root-relative.
- **Namespaces (Decision A rename):** root namespace = project name; update `namespace` decls,
  `using`s, and any `<Using Include="..."/>` in dependents' csprojs (e.g. the UI csproj has
  `<Using Include="Concertable.Customer.Seed"/>` etc. — those are unaffected, but fixture
  namespaces are).
- **InternalsVisibleTo / AssemblyInfo:** check each moved fixture's `AssemblyInfo.cs` for IVT
  targets that name the old assembly (memory: IVT-for-tests, Castle proxy IVT).
- **Don't add additive EF migrations or touch seeders here** — pure project relocation.

## 5. Boundary & convention checks
- After the move, **no `api/Tests/` directory exists.**
- Each service `.slnx` references only its own + shared-kernel test projects; the umbrella
  references everything via service-rooted / `Shared/Tests/`-rooted paths.
- Service-owned fixtures reference shared kernel (`Concertable.Testing[.Integration]`) — never
  the reverse, and never another service's runtime (ARCHITECTURE.md: Contracts-only across
  services; `Concertable.Testing.Integration` cross-service mocks are the documented exception
  per Decision C).
- Re-read `ARCHITECTURE.md` if any cross-service test reference feels load-bearing.

## 6. Verification
- Per slice: `dotnet build <that service>.slnx` → 0 errors.
- Final: `dotnet build api/Concertable.slnx` → 0 errors; `dotnet build` each per-service `.slnx`
  → 0 errors.
- `grep -rnE "Tests/Concertable\.|[\\\\/]\.\.[\\\\/]Tests[\\\\/]" api --include=*.csproj --include=*.slnx --include=*.targets --include=*.props` → **no hits** (no path points into the deleted `api/Tests/`).
- `find api/Tests` → does not exist.
- **After the full sweep, run the `e2e-ui-regress` skill** → no regression vs the baseline in
  `api/Tests/Concertable.E2ETests/E2E_BASELINE.md` (note: that baseline file moves with
  `Concertable.E2ETests` to `api/Shared/Tests/` in Slice 5 — point the skill at the new location).
- VS Test Explorer discovers the same test count as before (use `reset-test-explorer` skill if stale).
