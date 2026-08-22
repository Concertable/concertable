# app/shared — Technical Debt

## LOW

### API modules repeat their resource's base route as a string literal per method instead of a `BASE` const

`artistApi.ts`, `messageApi.ts`, `stripeAccountApi.ts`, and `venueApi.ts` each call `apiClient` with the
same route prefix (e.g. `"/message/..."`) written out fresh in every method instead of declared once as
a `const BASE = "/message"` and interpolated. A rename of the resource's route needs a find-and-replace
across every method instead of one edit; a copy-paste method more easily typos the prefix than a shared
constant would.

Found and fixed as `const BASE = "/Moderation"` / `const BASE = "/Admin"` +
`const INVITATION_BASE = "/AdminInvitation"` in the admin console's `moderationApi.ts`/`adminApi.ts`
(`app/web/admin/src/features/{moderation,admins}/api/`) — this entry is the pre-existing debt those two
were about to repeat before being written the other way.

**Resolves when:** each listed file gets a `const BASE = "/..."` (or one const per distinct resource
prefix, where a module genuinely calls more than one) at the top, with every method interpolating it
instead of restating the literal.

### Hand-rolled `useState` + manual zod `safeParse` per field, where `react-hook-form` is now the standard

`useReportMessage.ts`'s `ReportBuffer` (consumed by `ReportMessageDialog.tsx`, re-exported from
`features/messaging/index.ts`) hand-rolls the write-boundary pattern: a `useState` per field, a manually
named pre-validation type (`XBuffer` — "buffer" also reads as a binary/IO concept, which this same
codebase already uses correctly elsewhere for `ArrayBuffer` reads in `concertApi.ts`/`blobApi.ts`), and
a facade hook exposing `validate`/`submit` that calls `schema.safeParse` by hand. `react-hook-form` +
`@hookform/resolvers/zod` does this same job with less code and no separate draft type at all: the form
owns its own field state, `zodResolver` runs the schema, and `formState.errors`/`isValid` replace the
hand-rolled parse-on-every-render.

Migrated in the admin console's own copy of this pattern (`InviteForm`/`useInviteAdmin`,
`ResolveReportDialog`/`useResolveReport` — `app/web/admin/src/features/{admins,moderation}/`) when this
was raised; that's the model to follow. Sibling debt in `app/web/b2b/shared` (`InviteForm`/
`useInviteMember`, `OrganizationForm`/`useOrganization`) and `app/web/customer` (`useAddReview`) covers
the rest of the codebase. `useESignature.ts`'s canvas-drawn signature isn't a `react-hook-form` field in
the usual sense (it's not a text/select input) — worth a second look when this is tackled, but likely
stays a `useState`, just renamed away from "buffer".

**Resolves when:** `ReportMessageDialog`/`useReportMessage` uses `useForm` + `zodResolver` the same way,
`ReportBuffer` and the manual `validate`/`safeParse` call disappear entirely rather than getting renamed.

---
