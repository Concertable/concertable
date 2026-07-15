# Self-naming `OrNotFound` — pivot from `static virtual IEntity.DisplayName` to a `[DisplayName]` attribute

**Own branch off `master`: `Refactor/DisplayNameAttribute`.** Codebase-wide cleanup. Delivery is inherently
**≥2 merges** because `Concertable.Kernel` is a published package: land the Kernel mechanism first
(republishes on merge to `master`), then migrate each consuming service against the published version.

> **Status: designed, ready to implement. Supersedes the `static virtual IEntity.DisplayName` mechanism
> from the prior version of this plan (which is partially built, UNCOMMITTED, in the working tree).**

---

## Why pivot at all (the honest case)

The guard consolidation itself is settled and mostly landed: `.OrNotFound()` / `.OrNotFound(label)` over
`Task<T?>`, the Shape-A vs Shape-B split, the label/struct overloads, the bespoke-message allowlist, the
verbatim-match rule. **None of that changes.** What changes is *only how a type carries its own name* for
the zero-arg self-naming overload.

The prior mechanism — `static virtual string DisplayName` as a **default interface member** on `IEntity` —
was shipped as an explicit **temporary workaround** (see its own write-up: `static abstract` was
un-mergeable across the published-package boundary, two red CI runs with `TypeLoadException`, so it fell
back to a *soft* default member). Three costs of that workaround, all removed by the attribute:

1. **It leaks a Domain marker into Contracts.** To carry a name, `ArtistView` / `VenueView` (in
   `*.Contracts`) and the `ReadModels` were made to implement `Concertable.Kernel.IEntity` — the entity
   marker — purely for the name. A Contracts read-shape is not an entity; that implementation is a smell.
2. **Soft guarantee.** An un-overridden entity falls to a throwing default at *runtime*, not a compile
   error — the "every entity is named" standard is unenforced.
3. **Binary-compat landmine.** A member on the published `IEntity` is the exact thing that caused the
   `TypeLoadException` saga; every future change to it is package-version-sensitive.

The attribute (`[DisplayName("Venue")]`, read by cached reflection) removes all three: read off **any**
type — entity, Contracts View, ReadModel, DTO — with no `IEntity` coupling; no interface member,
so no binary-compat landmine; enforcement moves to an **architecture test** (a red test, not a silent
runtime throw). (`[DisplayName]`'s `AttributeUsage` excludes `Interface`, so the name is read off the
**concrete class** only — every self-naming type in this codebase is a class, so this costs nothing.)

**The trade accepted (decided):** cached reflection on the throw path (one `ConcurrentDictionary<Type,string>`
miss per distinct type, ever, on an already-exceptional path) in exchange for the three wins above; and the
compiler-nudge is recovered via an arch-test rather than a soft runtime default. Attribute chosen over a
bespoke `[DiagnosticName]` because the name doubles as a genuine display name, so `System.ComponentModel`'s
`[DisplayName]` is honest and needs no new type; its only other reader (ASP.NET/Swagger) would surface the
*same correct* string, so the "collision" is benign.

---

## Target mechanism

### Kernel — the resolver + the reconstrained guard

```csharp
// Concertable.Kernel — new: cached type→name resolver
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;

public static class DisplayNameResolver
{
    private static readonly ConcurrentDictionary<Type, string> Cache = new();

    public static string Of<T>() => Cache.GetOrAdd(typeof(T), Resolve);

    // [DisplayName] cannot target an interface (AttributeUsage excludes Interface); GetCustomAttribute's
    // default inherit:true still follows base CLASSES, which is all we need — every named type is a class.
    private static string Resolve(Type t)
        => t.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName
           ?? throw new InvalidOperationException(
               $"{t.Name} has no [DisplayName]; add one so OrNotFound can name it.");
}
```

```csharp
// Concertable.Kernel.Exceptions.NotFoundException.cs — reconstrain the self-naming overload
public static class NotFoundExtensions
{
    // was: where T : class, IEntity  =>  $"{T.DisplayName} not found"
    public static async Task<T> OrNotFound<T>(this Task<T?> task) where T : class
        => await task ?? throw new NotFoundException($"{DisplayNameResolver.Of<T>()} not found");

    // UNCHANGED — DTOs/projections + id-bearing/contextual messages (name is irreducible here)
    public static async Task<T> OrNotFound<T>(this Task<T?> task, string entity) where T : class
        => await task ?? throw new NotFoundException($"{entity} not found");

    // UNCHANGED — value-type id projections (Guid?/int?) a `where T : class` helper can't touch
    public static async Task<T> OrNotFound<T>(this Task<T?> task, string entity) where T : struct
        => await task ?? throw new NotFoundException($"{entity} not found");
}
```

- Drop the `IEntity` constraint from the zero-arg overload (`where T : class, IEntity` → `where T : class`).
  No call-site signature changes; `.OrNotFound()` reads identically.
- **Remove `static virtual DisplayName` from `IEntity`.** `IEntity` returns to the pure marker it was
  (still the base of `IEntity<TKey>` → `IGuidEntity`, still used as a data-access constraint) — only the
  `DisplayName` member goes.

### Each type — attribute instead of static member

```csharp
[DisplayName("Venue")]                     // was: public static string DisplayName => "Venue";
public sealed class VenueEntity : IGuidEntity { ... }

[DisplayName("Artist")]                     // Contracts View: ALSO drop `: IEntity` if it existed only for the name
public sealed class ArtistView { ... }
```

Shared-const sites (keyed-strategy pattern) — attribute args must be compile-time constants:

```csharp
// DealMetadata.cs / ConcertMetadata.cs — property → const
public static class DealMetadata { public const string DisplayName = "Deal"; }  // was: => "Deal";

[DisplayName(DealMetadata.DisplayName)]     // now legal: const, evaluated at compile time
public sealed class DealEntity : IGuidEntity { ... }
```

---

## Design decisions carried over unchanged (from the guard consolidation)

- The **call-site shape** `.OrNotFound()` / `.OrNotFound(label)` — the name now comes from an attribute
  instead of a static member, but the call site is byte-identical.
- The **label** and **struct** overloads, the **Shape-A/Shape-B** split, the **bespoke-message allowlist**
  (`StripeHoldClient` "No held payment intent…", `ContractAccessor` "No contract with id…", etc.), and the
  **verbatim-match rule** (self-name only where the message equals `"{DisplayName} not found"` exactly).
- The **`ThrowIfNull`** statement-form guards — complementary, untouched.

Only two things change per type: `static string DisplayName => "X"` → `[DisplayName("X")]`, and any
`: IEntity` added *solely* to carry that name is removed.

---

## Merge / phase plan (expand-contract, ≥2 merges)

### Phase 0 — DONE: clean slate (old design stashed)
The prior mechanism's migration (built on `static virtual IEntity.DisplayName`) was **uncommitted on
`master`** and has been **stashed** — recover with `git stash pop` (message: "old-design DisplayName
migration … superseded by [DisplayName] attribute pivot"). `master`'s working tree is now clean; we
reimplement fresh under the attribute design rather than untangling the old tree.
- The stash is a **reference only** — if reconstructing the ~75 call-site edits is faster by reading
  `git stash show -p stash@{...}` than re-deriving them, do so; otherwise implement per the plan.
- Two untracked orphan files remain in the tree (`DealMetadata.cs`, `ConcertMetadata.cs`) — leftover from
  the old work; they'll be authored properly (as `const`) in Phase 2. The untracked `reviews/*.md` is
  unrelated — leave it.
- **First real step of Phase 1: create branch `Refactor/DisplayNameAttribute` off clean `master`.**

### Phase 1 — Kernel (republishes; no consumer touched) — ✅ DONE (branch `Refactor/DisplayNameAttribute`)
1. ✅ Add `DisplayNameResolver` (cached, walks type + interfaces, throws on missing).
2. ✅ Reconstrain zero-arg `OrNotFound` to `where T : class`, body → resolver.
3. ✅ Remove `static virtual DisplayName` from `IEntity` (back to a pure marker).
4. ✅ Kernel unit tests: attribute on class resolves; attribute inherited via base class resolves;
   missing attribute throws `InvalidOperationException`; resolver caches (white-boxes the private cache
   dictionary — reference-equality is vacuous since attribute args are interned literals).
5. ✅ **Gate:** `dotnet build api/Concertable.slnx` clean (exit 0) + Kernel unit tests (14 passed).
   Behaviour-preserving. **Must merge and republish before any consumer migrates.**

### Phase 2 — consumer migration — ✅ DONE (branch `Refactor/DisplayNameConsumers`, off the `.571` platform-sync)
Kernel Phase 1 published `0.1.0-alpha.0.571`; the platform-sync bumped `ConcertablePlatformVersion` to it
across all five services. On top of that pin:
1. ✅ Replaced `static string DisplayName => "X"` with `[DisplayName("X")]` on the **8** self-named
   domain entities — the only types actually reached by a zero-arg `.OrNotFound()`:
   `ArtistEntity`, `ApplicationEntity`, `BookingEntity`, `ConcertEntity`, `ContractEntity`,
   `OpportunityEntity` (B2B Concert), `VenueEntity` (B2B Venue), `PreferenceEntity` (Customer Preference).
2. ✅ `DealMetadata` / `ConcertMetadata`: these were **unreferenced orphan files** — nothing read
   `<Metadata>.DisplayName`, and neither `DealEntity` nor Customer `ConcertEntity` is self-named via a
   zero-arg `.OrNotFound()`. So there was no keyed-strategy const to preserve; **deleted them as rot**
   rather than converting to `const` + annotating.
3. ✅ No Contracts `View`/`ReadModel` carried `: IEntity` **only** for the name. (Search's `*ReadModel`s
   list `IEntity`, but redundantly — `IIdEntity : IEntity<int> : IEntity` already implies it, and they're
   genuine persisted markers, not self-named — so left as-is.)
4. ✅ Call sites unchanged — `.OrNotFound()` already in the tree; only the name source changed.
5. 🔴 **REMAINING — arch-test.** Not yet added: a reflection `[Fact]` asserting every entity reached by a
   zero-arg `.OrNotFound()` carries `[DisplayName]`, so a future un-annotated entity fails a red test
   rather than only at runtime. This is the compiler-nudge the pivot traded away and the last open item.
   (Decide per-service vs one shared test; the self-named types live in B2B + Customer domain assemblies.)

Scope reality: the "~37 types / ~45 sites" estimate was inflated — only **8** types self-name via the
zero-arg overload; every other service (Auth / Payment / Search / Messaging) uses only the `(label)` /
struct overloads or none, so needed nothing but the version bump.

**Gate:** `dotnet build api/Concertable.slnx -c Release` clean (0 errors). Behaviour-preserving (same 404
`detail` text — `[DisplayName]` yields the identical string the static member did) → integration tests via
CI, **skip E2E**.

---

## Feasibility notes / gotchas
- **Attribute args are compile-time constants** — the only reason `DealMetadata`/`ConcertMetadata` need
  property→`const`. Any *computed* display name (none exist today) could not move to an attribute and would
  keep the label overload.
- **No interface annotation** — `[DisplayName]`'s `AttributeUsage` is `class, method, property, indexer,
  event`; it **cannot** be placed on an interface, so the name is read off the concrete class only. Every
  self-naming type here is a class, so nothing is lost. A future facade that returns only an interface
  (`Task<IDeal?>`) would use the `.OrNotFound(label)` overload instead.
## Done when
✅ `grep -rniE "static.*string DisplayName"` over `api/` returns zero (no metadata `const`s were needed);
✅ `static virtual DisplayName` is gone from `IEntity`; ✅ no `: IEntity` remains on a Contracts
View/ReadModel that carried it only for the name; ✅ every self-naming type has `[DisplayName]`.
🔴 **Outstanding: the arch-test** (see Phase 2, item 5) — the one thing left before this plan is deletable.
Delete this plan in the commit that adds it.
