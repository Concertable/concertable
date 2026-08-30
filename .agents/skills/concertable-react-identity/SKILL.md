---
name: concertable-react-identity
description: Concertable's frontend identity composition — the universal `User` in shared carries only id, email, verification and universal profile fields with no `venueId`, `artistId`, memberships or product subtypes, while `B2bIdentity extends User` in the b2b `tenant` feature adds memberships populated by a B2B-owned typed `/auth/me` query, read from that module and never cast off the shared type, mirroring the backend `ICurrentUser` and `ITenantContext` split exactly. Use when adding a field to a shared identity type, reading memberships, or composing a per-product identity.
---

# Identity — universal `User` in shared, `B2bIdentity` composed in b2b

The generic standard is the `tiered-shared-code` skill: identity is composed as per-product layers over a base
intersection type, never widened with product-specific fields or subtypes. This is what that resolves to here.

## The base intersection

`@concertable/shared` owns `User`: `id`, `email`, `isEmailVerified`, universal profile fields.

**No `venueId`, no `artistId`, no `memberships`, no product subtypes.**

## B2B composes its own view

`@b2b/*`'s `features/tenant` owns it:

```ts
B2bIdentity extends User   // adds: readonly memberships: ReadonlyArray<Membership>
```

populated by a **B2B-owned, typed `/auth/me` query** (`features/tenant/api/identityApi.ts`) returning what the
B2B backend actually sends. B2B code reads memberships from *that* module — never off the shared `User`, and
never by casting a field the shared type does not declare. `app/mobile/b2b` declares its own equivalent in
`navigation/identity.ts`.

`@concertable/customer/*` composes its own buyer identity the same way if it ever needs more than the base.

## This mirrors the backend split exactly

`ICurrentUser` in Kernel carries only `Id`, `Email` and `IsAuthenticated`; the tenant concept lives in a
separate `ITenantContext` that only B2B depends on. Widening either side is the same mistake in two languages.
