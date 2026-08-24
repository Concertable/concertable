# Code review — Feature/launch_tenant-verification

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `20a5061f18fa9f8f8c182de10993d1e8f164f525`  _(2026-08-24)_

> Range reviewed: `f033dc7e..20a5061f1` (2 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **NAT1 — MEDIUM — native (correctness)** — `TenantVerificationChangedDomainEvent.cs`
  The event carried a live reference to the mutable `TenantVerificationEntity` instead of snapshotting the
  fields it represents. Since `EventRaiser` accumulates events in a list dispatched later, two transitions
  raised on the same instance before dispatch would both end up reporting the entity's *final* state rather
  than the state at each raise. The Tenant module's own precedent (`TenantCreatedDomainEvent`,
  `TenantInvitationCreatedDomainEvent`) already snapshots primitives — Venue's `VenueChangedDomainEvent`
  carries the live entity, which is what this had mirrored instead.
  **Fix:** the event now carries `TenantVerificationId`/`TenantId`/`Status`/`RejectionReason`/
  `ReviewedByAdminSub`/`ReviewedAt` as primitives, raised via a new `Announce()` called at the end of each
  mutating method (`Submit`/`Resubmit`/`Approve`/`Reject`) once every field for that operation is set.
  Covered by `Reject_ThenResubmit_EachRaisedEventKeepsItsOwnSnapshot`.

- [x] **NAT2 — MEDIUM — native (error handling)** — `TenantVerificationEntity.cs` (`Reject`),
  `VerificationDocumentEntity.cs` (`Create`)
  Neither `Reject(reason)` nor `VerificationDocumentEntity.Create(blobName)` validated length against the
  EF-configured `HasMaxLength(1000)`/`HasMaxLength(500)`, so an over-length value would throw a raw SQL
  truncation exception at `SaveChanges` instead of a clean `DomainException`. `TenantEntity.UpdateLegalDetails`
  (200-char `LegalName`) and `TenantActivityEntity.Create` (truncates to match column lengths) both enforce
  this in the domain method.
  **Fix:** both now throw `DomainException` when the value exceeds its configured column length. Covered by
  `Reject_ReasonTooLong_ThrowsDomainException` and `Create_BlobNameTooLong_ThrowsDomainException`.

No further findings. Checked lenses: A (correctness — clean beyond NAT1/NAT2), B (service isolation — N/A,
no cross-service calls), C (module boundaries — clean, entirely within the Tenant module), D (seeding — N/A,
no seeder touched), E (`csharp-naming`, `csharp-style`, `dotnet:persistence`/`dependency-injection`,
`dotnet:multitenancy` — confirmed `TenantDbContext` is unscoped and sibling entities aren't `ITenantScoped`,
so no new stance was needed — `dotnet:domain-events`, `dotnet:unit-testing` — all clean), F (test coverage —
every success and failure branch on every new method has a covering test).
