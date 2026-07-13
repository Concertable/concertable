# Investigation: eliminate the repeated `?? throw new NotFoundException("X not found")` boilerplate — elegantly

## Your task

Propose the **cleanest modern-C# way** to express "load an entity, or 404 if it's not there",
so that it is used consistently across a large .NET 10 codebase **without hand-typing a redundant
per-call string** and **without hacky runtime type-name surgery**. Investigate real-world patterns,
weigh them, and recommend one (with a fallback). Produce copy-pasteable code. Do **not** just restate
the options already rejected below.

## Context

- **Stack:** C# / .NET 10, modular-monolith-of-microservices. Several independent services
  (`B2B`, `Customer`, `Payment`, `Search`, `Auth`) each reference a shared `Concertable.Kernel`.
- **`NotFoundException`** lives in `Concertable.Kernel.Exceptions`, is referenced by *every* service,
  and maps to an HTTP **404 ProblemDetails** (`HttpException` base; `Title = "Not Found"`, the
  constructor arg becomes the response **`detail`** — i.e. the message IS user/consumer-facing over HTTP).
- The pattern appears **60+ times** across all services, almost always in this exact shape:

  ```csharp
  var agreement = await repository.GetByApplicationIdAsync(id)
      ?? throw new NotFoundException("Booking agreement not found");
  ```

  Repos return `Task<TEntity?>` (nullable). The result must come back **non-null and inline** so it
  chains: `.ToDto()`, `.ToFileDownload(...)`, passed straight into another call, etc.

- Entity types are named with an **`Entity` suffix** by convention (`BookingAgreementEntity`,
  `VenueEntity`, `EscrowEntity`, …) and implement a marker like `IIdEntity` (int key) / `IGuidEntity`.
- A minority of call sites need an **id in the message**: `"Escrow {escrowId} not found"`,
  `"Contract {id} not found"`. A few are genuinely differently-worded (`"Cannot find ticket"`,
  `"No held payment intent found for application {applicationId}"`) and can keep an explicit message —
  they are NOT the target of this cleanup.
- Existing helper already on the exception (kept for the guard-*statement* case, not the inline-expression case):

  ```csharp
  public static void ThrowIfNull(
      [NotNull] object? argument,
      string? message = null,
      [CallerArgumentExpression(nameof(argument))] string? paramName = null) { ... }
  ```

## What "good" means (hard constraints)

1. **No redundant magic string at the call site** in the common case. Typing `"Booking agreement"`
   when you're already calling `GetByApplicationIdAsync` on a `Task<BookingAgreementEntity?>` is the
   smell we are trying to remove.
2. **No hacky type-name string surgery** (e.g. `typeof(T).Name.EndsWith("Entity") ? name[..^6] : name`).
3. **Must NOT leak the `Entity` suffix** (or other internal naming) into the HTTP 404 body. The
   message a consumer sees should be reasonable (`"Booking agreement not found"`-ish), or the design
   should make a deliberate, defensible choice about what that message is.
4. **Composes cleanly with `await`** — no `(await ...)` wrapper parens at the call site. Returning an
   extension over `Task<T?>` is acceptable and encouraged if it reads well.
5. **Returns the non-null value** so it chains inline.
6. Lives in shared `Kernel` (all services use it). Cheap at runtime; reflection per-throw is
   acceptable (throw is the cold path) but reflection-as-string-hack (#2) is not.
7. Idiomatic and boring to read. A junior should understand the call site instantly.

## Approaches already considered and REJECTED (don't re-propose these as the answer)

- **Explicit label every call** — `OrNotFound("Booking agreement")`. Rejected: that's the redundant
  string we're removing. (It may still be a *fallback* for the id-bearing minority — that's fine.)
- **`typeof(T).Name` with `"Entity"`-suffix stripping** — hacky (#2), and prettifying a type name is fragile.
- **`typeof(T).Name` verbatim** — leaks `"Entity"` into the 404 body (#3): `"BookingAgreementEntity not found"`.
- **Generic `"Resource not found"`** — loses the (occasionally useful) hint about what was missing;
  acceptable only if you argue it's genuinely the right call.

## Directions worth investigating (not exhaustive — find better)

- A **display-name source on the entity** that isn't string surgery: e.g. C# 11 **`static abstract`
  interface members** (`static abstract string EntityName { get; }` on the entity marker interface),
  or a `[DisplayName]`/attribute read once and cached per type, or a small source generator.
- Centralising in the **repository base** (`GetByIdRequiredAsync` / a `Required(...)` wrapper) so the
  throw + naming live in one place the call sites inherit, rather than at every service method.
- **Result/typed-error** patterns (the codebase already uses FluentResults `Result<T>` in some
  services) vs exceptions — is 404-as-exception even the right model here, or should these be
  `Result<T>` with a `NotFound` error the controller translates? Weigh the two honestly.
- Off-the-shelf: `Ardalis.GuardClauses`, `ErrorOr`, etc. — only if they genuinely beat a 10-line
  in-house helper for THIS constraint set.

## Deliverable

1. A short ranked recommendation (best + one fallback) with the tradeoffs each constraint above forces.
2. Final code for the chosen helper(s), placed as it would live in `Concertable.Kernel.Exceptions`.
3. 2–3 representative **before/after** call sites (including one id-bearing case and one `.ToDto()`
   chain), showing the exact resulting 404 message string for each.
4. A one-line note on how the ~60 existing sites would be migrated (and which ones intentionally keep
   an explicit message).
