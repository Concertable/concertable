---
name: concertable-react-write-boundary
description: Where Concertable's zod schemas live — per feature in `features/<feature>/schemas/`, with `SearchSchema` the precedent to copy, and zod already being the project's validation tool through TanStack `validateSearch` so a form schema adds no dependency. Use when adding a form schema here, or deciding where a new one belongs.
---

# Forms — where Concertable's zod schemas live

The generic standard is the `write-boundary` skill: validate the controlled-input buffer against a zod schema
at submit, map the *parsed* result to the request type, tie the two with `z.infer` so drift is a compile
error.

Schemas live per feature, in `features/<feature>/schemas/`.

`SearchSchema` in `features/search/schemas/` is the precedent to copy. zod is already the project's validation
tool through TanStack's `validateSearch`, so a form schema adds no dependency.
