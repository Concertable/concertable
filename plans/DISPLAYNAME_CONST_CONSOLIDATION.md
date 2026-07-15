# Self-naming `OrNotFound` — centralise the display names in per-module `DisplayNames` consts

**Fresh-context implementation plan.** Own branch off clean `master` (e.g. `Refactor/DisplayNameConsts`),
own PR, normal merge queue (this touches **no** Kernel code — purely consumer-side — so it's a single
non-breaking PR, no expand/contract, no `--admin`).

## Context — what already shipped (don't redo)

- **#105 (merged):** `Concertable.Kernel` gained `DisplayNameResolver.Of<T>()` (cached reflection reading
  `System.ComponentModel.DisplayNameAttribute`), the zero-arg `OrNotFound<T>()` was reconstrained to
  `where T : class` and now resolves the name via the attribute, and `static virtual IEntity.DisplayName`
  was removed. Published as `0.1.0-alpha.0.571`.
- **#107 (merged):** all services bumped to `.571`; the **8** entities reached by a zero-arg `.OrNotFound()`
  got `[DisplayName("X")]`; the unreferenced `DealMetadata`/`ConcertMetadata` orphans were deleted.

So today the mechanism works, but the **name strings are still scattered as literals** — on each entity
(`[DisplayName("Venue")]`) and at every label-form call site (`.OrNotFound("Venue")`).

## Goal

One **single source of truth** per concept. Each module gets a `public static class DisplayNames` of
`const string`s in its **Contracts** project; every `[DisplayName(...)]` and every value-type/contextual
`.OrNotFound(...)` references a const. Read DTOs get decorated so their fetches drop to zero-arg. No magic
strings left in `OrNotFound` calls.

## Locked decisions

- **Framework `[DisplayName]`** (not a bespoke attribute) — no new Kernel type, doubles as the ASP.NET/Swagger
  display name. (The "is this the right call?" critique lives in `plans/DESIGN_REVIEW_SELF_NAMING.md`; this
  plan assumes we keep the framework attribute.)
- **Holder named `DisplayNames`, one per module** — each module owns its own `DisplayNames` in its own
  `Contracts` project. **B2B and Customer never share a holder** (microservice boundary), and within B2B
  the modules don't share one either.
  - **The CS0104 trap and the fix (namespace placement, no shared holder).** The Concert module's
    `GlobalUsings` (Application + Infrastructure) blanket-import `Artist.Contracts` + `Venue.Contracts` +
    `Concert.Contracts` (for `ArtistSummary`/`VenueSummary`/`IArtistModule`/`IVenueModule`). If each
    module's `DisplayNames` sits in its `…{Module}.Contracts` namespace, all three land in scope together
    and bare `DisplayNames` is ambiguous (CS0104) in every Concert file. **Fix:** declare each module's
    holder in the **module-root namespace** — `namespace Concertable.B2B.Concert;` (not
    `…Concert.Contracts;`), likewise `Concertable.B2B.Artist` / `.Venue` / `.Conversations`. C# resolves
    an unqualified name from *enclosing* namespaces, so every file under `Concertable.B2B.Concert.*` sees
    `Concertable.B2B.Concert.DisplayNames` with **no `using` at all**; and Concert importing
    `Concertable.B2B.Artist.Contracts` does **not** pull in the parent `Concertable.B2B.Artist` (a `using`
    imports one namespace, not its ancestors), so Artist's/Venue's holders never enter Concert's scope.
    Exactly one `DisplayNames` visible in any file → bare `DisplayNames.X` everywhere, **no alias, no
    fully-qualified name, no shared project.** The holder still lives in the module's `Contracts` project
    (its published assembly) — only the declared namespace differs from the folder.
  - **Rejected:** a shared `Concertable.B2B.Contracts` holder that all modules reference. It compiles, but
    it's a cross-module junk-drawer — module-owned concepts (`Booking`, `Opportunity`, `Contract`) pooled
    service-wide and published cross-service — which defeats the module isolation this codebase is built on.
  - **Customer: same per-module shape.** No collision there regardless — Customer files use per-file
    `using`s, never a blanket global-import of two holder namespaces — so Customer's holders stay in their
    `…{Module}.Contracts` namespaces as-is.
- **Members are the concept nouns** (`DisplayNames.Concert`, `DisplayNames.Booking`) — no `.DisplayName`
  suffix, no class-per-concept.
- **`SetupCheckoutStep` unifies to `"Concert Opportunity"`** (it currently says the inconsistent
  `"Opportunity"`; the entity + `ApplyExecutor` already say "Concert Opportunity" — more readable, fixes the
  inconsistency). Intentional 404-text change.
- **`ApplicationNotifier` unifies to `"Application"`** via `DisplayNames.Application` (404 text
  `"Concert application"` → `"Application"`). The bespoke `ConcertApplication` const was dropped —
  the notifier is talking about the same `ApplicationEntity` every other call site names `"Application"`,
  so a separate string was just the inconsistency this plan removes. Second intentional 404-text change.
- **Every module should have a `Contracts` project; add one where a module we touch lacks it.** Here that's
  **`Concertable.Customer.Preference.Contracts`** (Preference currently has only Api/Application/Domain/
  Infrastructure). Mirror an existing Customer module's Contracts `.csproj` (e.g. `Ticket.Contracts`), add it
  to the solution, and reference it from Preference's Domain (and Application/Infrastructure/Api as needed).

## Placement rule (why Contracts)

Contracts is the leaf every layer already references (Domain → Contracts, Application → Contracts,
Infrastructure → Contracts), so the entity attribute, the DTO attributes, and the call sites can all see the
const. **Verify** each module's `Infrastructure` (and `Application`/`Domain`) actually references its own
`Contracts` before relying on it — it held for Artist/Concert/Venue/Conversations/Customer-Concert/Ticket
when this plan was written.

## New files — 7 `DisplayNames.cs` (one per module, in each module's Contracts project)

**B2B** — declared in the **module-root namespace** (`Concertable.B2B.{Module}`, not `…{Module}.Contracts`)
so every layer sees it via enclosing-namespace resolution while cross-module imports don't collide
(see Locked decisions):
| Project (namespace) | Consts |
|---|---|
| `Modules/Artist/…Artist.Contracts` (`Concertable.B2B.Artist`) | `Artist = "Artist"` |
| `Modules/Concert/…Concert.Contracts` (`Concertable.B2B.Concert`) | `Concert`, `Application`, `Booking`, `Contract`, `Opportunity = "Concert Opportunity"` |
| `Modules/Venue/…Venue.Contracts` (`Concertable.B2B.Venue`) | `Venue = "Venue"` |
| `Modules/Conversations/…Conversations.Contracts` (`Concertable.B2B.Conversations`) | `MessageSender = "Message sender"` |

No `ConcertApplication` const: `ApplicationNotifier` now uses `DisplayNames.Application` (404 text
`"Concert application"` → `"Application"`, a second intentional text change alongside `SetupCheckoutStep`).

**Customer** — per-module in the module's `Contracts` namespace (no collision, so no root-namespace trick needed):
| Project | Consts |
|---|---|
| `Modules/Concert/…Customer.Concert.Contracts` | `Concert = "Concert"` |
| `Modules/Ticket/…Customer.Ticket.Contracts` | `QrCode = "QR Code"` |
| `Modules/Preference/…Customer.Preference.Contracts` **(NEW project)** | `Preference = "Preference"` |

Shape (B2B Concert; note the module-root namespace):
```csharp
namespace Concertable.B2B.Concert;

public static class DisplayNames
{
    public const string Concert = "Concert";
    public const string Application = "Application";
    public const string Booking = "Booking";
    public const string Contract = "Contract";
    public const string Opportunity = "Concert Opportunity";
}
```

## Entities — literal → const (8)

Convert `[DisplayName("X")]` → `[DisplayName(DisplayNames.X)]`. No `using` needed for B2B: the holder's
module-root namespace (`Concertable.B2B.{Module}`) is an ancestor of every file in the module, so it
resolves implicitly.

| Entity | Const |
|---|---|
| `ArtistEntity` (B2B Artist.Domain) | `DisplayNames.Artist` |
| `ApplicationEntity` (B2B Concert.Domain) | `DisplayNames.Application` |
| `BookingEntity` (B2B Concert.Domain) | `DisplayNames.Booking` |
| `ConcertEntity` (B2B Concert.Domain) | `DisplayNames.Concert` |
| `ContractEntity` (B2B Concert.Domain) | `DisplayNames.Contract` |
| `OpportunityEntity` (B2B Concert.Domain) | `DisplayNames.Opportunity` |
| `VenueEntity` (B2B Venue.Domain) | `DisplayNames.Venue` |
| `PreferenceEntity` (Customer Preference.Domain) | `DisplayNames.Preference` |

## DTOs — add `[DisplayName(DisplayNames.X)]` (6)

So their fetches can go zero-arg:

| DTO | Project | Const |
|---|---|---|
| `ArtistDetails` | B2B Artist.Application | `DisplayNames.Artist` |
| `ArtistSummary` | B2B Artist.Contracts | `DisplayNames.Artist` |
| `ConcertDetails` | B2B Concert.Application | `DisplayNames.Concert` |
| `VenueDetails` | B2B Venue.Application | `DisplayNames.Venue` |
| `VenueSummary` | B2B Venue.Contracts | `DisplayNames.Venue` |
| `ConcertDto` | Customer Concert.Contracts | `DisplayNames.Concert` |

## Call sites

**→ zero-arg** (fetch a now-annotated type; literal removed): `.OrNotFound("X")` → `.OrNotFound()`
- `ArtistService.cs:45` (ArtistDetails), `:116` (ArtistSummary)
- `ConcertService.cs:56`, `:62` (ConcertDetails)
- `VenueService.cs:46` (VenueDetails), `:129` (VenueSummary)
- `TicketService.cs:53`, `:127` (ConcertDto), `TicketValidator.cs:37` (ConcertDto)

**→ const** (value/tuple/contextual — no type to self-name; string centralised): `.OrNotFound("X")` → `.OrNotFound(DisplayNames.X)`
- `BookingService.cs:38` → `Application`
- `ContractIssuer.cs:43` → `Application`
- `Workflow/Executors/ApplyExecutor.cs:62`, `:67` → `Opportunity`
- `Workflow/Executors/SettlementExecutor.cs:33` → `Booking`
- `Workflow/Steps/CaptureEscrowAcceptStep.cs:38` → `Application`
- `Workflow/Steps/HoldCheckoutStep.cs:27`, `:29` → `Application`
- `Workflow/Steps/RefundEscrowStep.cs:20` → `Booking`
- `Workflow/Steps/ReleaseEscrowFinishStep.cs:20` → `Booking`
- `Workflow/Steps/VerifyCheckoutStep.cs:30`, `:33`, `:35` → `Application`
- `Workflow/Steps/SetupCheckoutStep.cs:35` → `Opportunity` **(intentional text change "Opportunity" → "Concert Opportunity")**
- `ApplicationNotifier.cs:63`, `:74` → `Application` **(intentional text change "Concert application" → "Application")**
- `Conversations/…/MessageService.cs:87` → `MessageSender`
- `Customer Ticket/…/QrCodeService.cs:27` → `QrCode`

(Line numbers are as of this plan — re-grep `\.OrNotFound\("` to confirm before editing.)

## Arch-test from the earlier plan — ✅ landed (partial coverage)

The **arch-test** from `plans/HTTP_GUARD_CONSOLIDATION.md` is now written:
`Concertable.B2B.Concert.UnitTests/DisplayNameConventionTests.cs` — a `[Theory]` driving
`DisplayNameResolver.Of<T>()` (which throws when `[DisplayName]` is absent) for each self-naming type,
pinning its resolved 404 label. A dropped attribute or a changed label fails red at test time, not only
on the runtime throw path.

Coverage is the B2B self-naming types **reachable from the Concert module test project**: all five Concert
entities (`Concert`/`Application`/`Booking`/`Contract`/`Opportunity`), `ConcertDetails`, and the two
cross-module summaries `ArtistSummary`/`VenueSummary`. **Not yet covered** (would need a fact in each of
those modules' own test projects — Artist/Venue have only IntegrationTests today): `ArtistEntity`,
`ArtistDetails`, `VenueEntity`, `VenueDetails`, and the Customer side. Low residual risk (single-const
modules); extend per-module if it's ever worth it. Delete `HTTP_GUARD_CONSOLIDATION.md` on merge.

## Verification gate

- `dotnet build api/Concertable.slnx -c Release` → 0 errors (new `Preference.Contracts` project must be in
  the solution and referenced).
- Integration tests via CI. Behaviour-preserving **except** the one intentional `SetupCheckoutStep` text
  change → run E2E through the merge queue (normal path; no bypass — this is a green PR).
- No model change → no migrations.

## Done when

`grep -rniE '\.OrNotFound\("' api` returns **zero** (every label call references a const); every
`[DisplayName(...)]` references a `DisplayNames` const (no string literals); Preference has a Contracts
project; build + tests green. Then delete this plan (and close out the arch-test item in
`HTTP_GUARD_CONSOLIDATION.md`, deleting that plan too if the arch-test landed here).
