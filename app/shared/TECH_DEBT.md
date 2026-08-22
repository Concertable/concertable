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

### The write-boundary pattern's pre-validation form state is named `XBuffer`, not `XDraft`

`useReportMessage.ts`'s `ReportBuffer` (consumed by `ReportMessageDialog.tsx`, re-exported from
`features/messaging/index.ts`) names the controlled-input state before it's parsed into a request
`XBuffer`. "Buffer" reads as a binary/IO concept (and this same codebase already uses it correctly that
way — `ArrayBuffer` reads in `concertApi.ts`, `blobApi.ts`); "draft" is the clearer, unambiguous name for
"user-entered values not yet validated or committed."

Renamed in the admin console's own copy of this pattern (`InviteDraft`, `ResolveDraft` —
`app/web/admin/src/features/{admins,moderation}/`) when this was raised; this entry is what's left.
Sibling debt in `app/web/b2b/shared` (`InviteBuffer`, `OrganizationBuffer`) and `app/web/customer`
(`ReviewBuffer`) covers the rest of the codebase.

**Resolves when:** `ReportBuffer` → `ReportDraft` (type, hook parameter names, the re-export), matching
the admin console's already-renamed pattern.

---
