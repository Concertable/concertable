# Code review — Chore/TechDebtEmptyStringLiterals

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `c0bc7d544a0ccc8775e3477796f5a57bac46365b`  _(2026-08-30)_
**Security-reviewed up to commit:** `c0bc7d544a0ccc8775e3477796f5a57bac46365b`  _(2026-08-30)_

> Security scope: `api/Concertable.Payment/TECH_DEBT.md` and the `ClientSecret`/proto changes matched the
> generic secret-vocabulary pattern. Traced every touched call site (`payment.proto`, both
> `Concertable.Payment.Infrastructure/Grpc/*Mappers.cs`, both `Concertable.Payment.Client/Adapters/*Mappers.cs`):
> the diff changes only how the *absence* of a Stripe client secret is represented on the wire (an
> ambiguous `""` sentinel → an explicit `HasClientSecret` presence bit via proto3 `optional`). The secret
> value itself is unchanged in transit (still TLS gRPC), never logged, never weakened, never newly exposed.
> No findings.

> Range reviewed: `8e74c4eee..c0bc7d544` (7 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **NAT1 — HIGH — native** — `api/Concertable.Payment/tests/Concertable.Payment.UnitTests/Compatibility/PublishedPackageCompatibilityTests.cs:41`
  Marking `client_secret` `optional` in `payment.proto` changes its `FieldDescriptorProto` shape
  (`proto3_optional`/synthetic oneof), so `ProtobufDescriptor_CurrentSchemaIsAdditive` fails against the
  baseline snapshotted from the last **published** version. Confirmed genuinely wire-compatible (proto3
  implicit vs. explicit-optional presence encode identically) but the baseline can only be regenerated
  from a real published artifact, never from this branch's own candidate. **Resolution:** logged as its
  own tech-debt entry in `api/Concertable.Payment/TECH_DEBT.md` ("Published-contract compatibility
  baseline is stale after the `client_secret` presence fix") with the exact re-baseline steps for the
  follow-up PR after this publishes; user explicitly authorized an admin merge past this one known-red
  check.

- [x] **CV1 — MEDIUM — convention (csharp-naming / constants placement)** — `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/StripePaymentIntentStatuses.cs`
  The new Stripe status constants class was first placed in `Infrastructure/Mappers/` — a subfolder tied
  to one caller — instead of the project root, where this codebase's own precedent puts a project-local
  constants class (`TransactionTypes.cs`, `PaymentMetadataKeys.cs` both sit at their owning project's
  root). Also renamed singular `StripePaymentIntentStatus` → plural `StripePaymentIntentStatuses` to
  match that same precedent (`TransactionTypes`, `PaymentMetadataKeys`, `RateLimitPolicies`). Moved to
  `Concertable.Payment.Infrastructure/StripePaymentIntentStatuses.cs` and every call site
  (`PaymentIntentMappers.cs`, `StripePaymentIntentClient.cs`, `FakeStripePaymentIntentClient.cs`, the new
  test file) updated.

- [x] **TEST1 — MEDIUM — test coverage (Lens F)** — `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/Mappers/PaymentIntentMappers.cs:16`
  The new boundary guard (`throw new InvalidOperationException` when Stripe returns no `PaymentIntent`
  id) is a brand-new branch introduced by this diff with no covering test — the only place in the
  codebase that validates the untyped Stripe SDK response before it becomes a `PaymentOutcome`. Added
  `api/Concertable.Payment/tests/Concertable.Payment.UnitTests/Infrastructure/PaymentIntentMappersTests.cs`
  covering the success path, both `RequiresAction`/`RequiresConfirmation` branches, the rejected-status
  failure, and the missing-id throw.

No other findings. Lenses checked: A (correctness), B (service isolation), C (module boundaries), D
(seeding — n/a, no seeders touched), E (language/framework conventions — csharp-naming, csharp-style,
dependency-injection, module-structure, persistence, result-carriers, unit-testing, proto,
react http-layer/contract-naming/typescript-style/tiered-shared-code/app-tiers, docs-and-debt), F (test
coverage). The `PaymentMethod | undefined` → `| null` frontend retype was checked against
`typescript-style`'s "absent values default to `undefined`" rule and confirmed to fall under its own
named exception: TanStack Query v5 throws on an `undefined` query-function resolution but not `null`, so
`null` is the only type that doesn't lie about a real runtime behavioral difference here.
