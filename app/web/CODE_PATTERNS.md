# Frontend Code Patterns

Recurring design patterns the web frontend commits to. When a change fits one of these shapes, use
the pattern — don't invent a local variant. Sibling of
[`CODE_CONVENTIONS.md`](./CODE_CONVENTIONS.md) (naming/style); this file is about **structure**.

It is the FE analog of [`api/docs/CODE_PATTERNS.md`](../../api/docs/CODE_PATTERNS.md), and every
pattern here has a backend sibling — the two stacks are meant to read the same way. Where a rule about
*where code lives* is owned by a tier's `CLAUDE.md`, this file names the **structural technique** and
links out rather than restating it (two copies drift the day one changes).

The tiers referenced throughout (`@concertable/shared` ⊃ `app/web/shared` ⊃ `@b2b/*` / `@customer/*`
⊃ per-app `src/`) are defined in [`CLAUDE.md`](./CLAUDE.md) and the per-tier `CLAUDE.md` files. Read
those first; this file assumes them.

---

## Shared is the intersection — vary it with slots, never with a role check

The backend's "shared is the intersection, never the union" ([`api/CLAUDE.md`](../../api/CLAUDE.md))
is the same rule the FE lives under, enforced by four `tsc -b` builds. This section is the
**structural technique** that keeps it true: when a shared surface must differ by product or persona,
the shared code declares a **slot** and the owning app **injects** the variation. It never learns who
it's rendering for.

- A shared component takes variation as props/slots: `AppLayout({ links })`,
  `ConcertDetails({ addReviewSlot, onBuyTickets })`, `OpportunitySection({ renderActions })`. Venue
  injects "View Applications", artist injects "Apply" — the section never branches.
- A **fixed** affordance stays declared in shared (`ConcertCard`'s Buy-Tickets button, disabled when
  no `onBuyTickets` is supplied); only the app-specific *behaviour or widget* is injected. Keep the
  shared UI intentional — a slot is for genuine per-app variation, not for punting every decision to
  the app.
- Identity-conditional composition is the **app's** job. The app knows its persona; it picks which
  slot contents to pass. Shared code receives the result, already decided.

```tsx
// CORRECT — shared declares the slot; the app decides what fills it
function OpportunitySection({ renderActions }: { renderActions: (o: Opportunity) => ReactNode }) { … }

// venue app
<OpportunitySection renderActions={(o) => <ViewApplications opportunity={o} />} />

// WRONG — shared code inspecting identity to branch
function OpportunitySection({ opportunity }: { opportunity: Opportunity }) {
  const { user } = useAuthStore();
  return isVenueManager(user) ? <ViewApplications … /> : <Apply … />; // tier leak
}
```

### The anti-patterns this replaces — never do these

- **A role check inside shared** (`isVenueManager(user)`, `role === "VenueManager"`,
  `activeTenant.type === ...`) to pick behaviour. This is the disease, not the cure: it makes shared
  code know its audience. Move the branch to the app and inject the result through a slot. This has
  caused real bugs (shared review widgets firing Customer-service calls with manager tokens — see
  [`shared/CLAUDE.md`](./shared/CLAUDE.md)).
- **Parking product code in a wider tier "for now."** B2B concepts (opportunities, contracts,
  deals, payouts) in `app/web/shared` or `app/shared`; a customer route literal in `app/web/shared`.
  The wider tier compiles it into apps that can never use it, and customers gained access to B2B
  surfaces exactly this way before the split. It goes in its owning tier from the first commit.
- **An app-specific route literal in a shared route contract.** Only literals *every* consumer of the
  tier registers may appear in that tier (the route rules in each `CLAUDE.md`); anything else is
  injected by the owning app.

---

## Identity is composed, never widened — base auth, per-product layers on top

The universal `User` in `@concertable/shared` models only what **every** surface has — the
intersection: `id`, `email`, `isAuthenticated`, universal profile fields. A product concept
(persona, tenant membership, buyer state) is **composed on top** by the product that owns it, never
bolted onto the shared type.

This is the direct FE mirror of the backend's identity split
([`api/CLAUDE.md`](../../api/CLAUDE.md), "Tenancy is composed, never subtracted"): `ICurrentUser`
(Kernel) carries only `Id`/`Email`/`IsAuthenticated`; the tenant/owner concept lives in a **separate
`ICurrentTenant` that only B2B depends on**. The FE does the same, one layer per product:

- **`@concertable/shared` — base identity.** `User` = the intersection. No persona subtypes, no
  `venueId`/`artistId`, no `memberships`.
- **`@b2b/*` — the B2B identity layer, composed on the base.** A single B2B identity module owns the
  B2B view of the signed-in user: base user + persona + `memberships`, populated by a **B2B-owned,
  typed `/me` query** (the payload the B2B backend actually sends). B2B code reads memberships from
  *this* module, never off the shared `User`.
- **`@customer/*`** composes its own buyer identity the same way if/when it needs more than the base.

Because the B2B identity module is stateful domain data (which tenant is active, what the memberships
are, is a choice pending), it is *also* the "one home for the domain's reactive state" (next section)
— the two patterns land on the same module.

```ts
// @concertable/shared — base only
export interface User { id: string; email: string; isEmailVerified: boolean; /* universal */ }

// @b2b/features/identity — composed on top, typed /me, no cast
export interface B2bIdentity { user: User; memberships: Membership[]; }
const b2bMeApi = { getMe: async (): Promise<B2bIdentity> => (await api.get<B2bIdentity>("/auth/me")).data };
```

### The anti-patterns this replaces — never do these

- **Persona subtypes in the universal union.** `User = VenueManager | ArtistManager | Customer` with
  `venueId?`/`artistId?` fields (`app/shared/src/features/auth/types.ts`) enumerates product personas
  in the tier a customer/mobile bundle compiles — dead weight for everyone but one persona. Personas
  compose in their own tier.
- **Casting extra fields off the shared `User`.** `membershipsOf(user)` reading a `memberships` field
  the type doesn't declare (`@b2b/features/tenant/lib/tenantChoice.ts`) is the shared type lying about
  its shape. The typed B2B `/me` removes the cast — the field is *typed where it's real*.
- **Two discriminants on one union.** `User` carrying both `$type` and `role`, with guards narrowing
  on `role` while the wire polymorphism key is `$type`, is a pick-one-key violation
  ([`CODE_CONVENTIONS.md`](./CODE_CONVENTIONS.md), "Polymorphic JSON"). A composed identity narrows on
  exactly one key.

---

## A domain's reactive state has exactly one home — co-locate by domain

A **feature is a slice**: everything one domain exchanges and owns lives together under
`features/<feature>/` — `types` / `api` / `hooks` / `components` / `pages` / `schemas`, **plus exactly
one home for the domain's reactive state** (a zustand store, the slice's `model`). This is the
consensus modern-React structure (feature-folder colocation; the same axis Feature-Sliced Design calls
a *slice* × *segments*), and it is the FE mirror of the modular monolith: one domain = one module,
state and behaviour co-located, one owner.

The rule is about **cohesion, not file count**. More files are fine; the defect is a single cohesive
*stateful* domain sprayed across disjoint owners that each re-derive the same thing.

- **Reactive mutable state → the one store.** One store per domain owns the mutable truth
  (`activeTenantId`, memberships, persona). Everything else reads *through* it.
- **Derived state → a selector or hook off that store, never stored.** "Is a choice pending", "the
  active membership", "can this role do X" are computed from the store, not persisted alongside it.
- **One core, two entry points.** When the same derivation is needed both imperatively (a route
  `beforeLoad` calling `store.getState()`) and reactively (a component via a hook), write the logic
  **once** and expose both a `getState`-based function and a hook over that single core. `beforeLoad`
  and render must never be able to disagree.
- **Genuinely stateless, reusable rulebooks stay pure — wherever they naturally live.** A pure
  `hasPermission(role, permission)` over a static matrix holds no state and is a *correct* pure
  function; it does not get dragged into the store. Co-location is about keeping the *stateful* domain
  cohesive, not about banning pure helpers.

```ts
// CORRECT — one core, one home; hook and imperative form share it
// @b2b/features/identity/model.ts
export const useIdentityStore = create<IdentityState>()(…);          // the one home
export const selectChoicePending = (s: IdentityState) => …;          // derived selector
export const getChoicePending = () => selectChoicePending(useIdentityStore.getState()); // beforeLoad
export const useChoicePending = () => useIdentityStore(selectChoicePending);             // render
```

### The anti-patterns this replaces — never do these

- **The same derivation implemented twice.** `tenantChoicePending` computed by an imperative
  `isTenantChoicePending()` in `lib/tenantChoice.ts` *and* a reactive `useTenantChoicePending()` in
  `hooks/useActiveMembership.ts`, with `_venue/route.tsx` calling both for the same question. Two
  copies, guaranteed drift. Collapse to one core.
- **Domain state scattered across store + loose fns + hooks + a cast.** The tenant domain today:
  `useActiveTenantStore` + `tenantChoice.ts` (imperative fns + cast) + `useActiveMembership.ts`
  (reactive hooks) + `tenantPermissions.ts` for one concern. Consolidate into the identity slice with
  one state home; keep only the genuinely-pure `tenantPermissions` rulebook separate.
- **Server data copied into a store.** `useConcertStore.draft = { ...concert }`
  (`app/shared/src/features/concerts/store`) snapshots cache data into global state, which breaks
  background refetch ([`CODE_CONVENTIONS.md`](./CODE_CONVENTIONS.md), "Mutation variables vs form
  state"). The live buffer is local `useState` in the editing component; the cache stays the source of
  truth.
- **Derived value maintained by hand as state.** `isDirty` recomputed in every setter is derived data
  held as state — derive it from buffer-vs-source at read time.

---

## Errors are handled once, at the query client

`ProblemDetails` ([RFC 9457](https://datatracker.ietf.org/doc/html/rfc9457): `{ title, detail,
errors[] }`) is handled in exactly one place — `QueryCache.onError` / `MutationCache.onError` in
`app/web/shared/src/lib/queryClient.ts`, the [TkDodo-recommended](https://tkdodo.eu/blog/react-query-error-handling)
global seam. A feature never re-resolves or re-toasts an API error; the client already did.

- The only opt-out is typed `meta` on the query/mutation: `silenceErrors`, or `expectedErrors: [404]`
  for a status the caller handles itself (registered via TanStack module augmentation — see the
  `ErrorMeta` declaration in `queryClient.ts`).
- The only place a feature legitimately inspects an error is to **change control flow, not to report**
  — e.g. a route guard doing `isAxiosError(e) && status === 401` to `throw redirect(...)`.

```ts
// CORRECT — the caller expects 404 and renders its own empty state; the client stays silent
useQuery({ queryKey, queryFn, meta: { expectedErrors: [404] } });

// WRONG — a second, generic toast on top of the one the client already fired
useMutation({ mutationFn, onError: () => toast.error("Failed to save.") });
```

### The anti-patterns this replaces — never do these

- **Feature-local `onError` toasts.** A per-mutation `onError: () => toast.error(...)` fires *on top
  of* the global `MutationCache.onError`, producing a **double toast** — an observable bug, not a style
  nit. Present in `members`, `organizations`, and the per-app concert-action hooks. Delete every one.
- **`try/catch` around `mutateAsync` to toast.** Same double-report, spelled with a catch. Catch only
  to change control flow (redirect, fallback), never to surface the error copy.
- **A validation message shown via `toast`.** "Please select a star rating" belongs inline next to the
  field, from the zod `safeParse` result — not a toast (see the write-boundary pattern below).

---

## Hooks orchestrate; components render

All logic — fetch, mutate, derive, orchestrate — lives in hooks. Components consume a hook and render.
This is the standard React data-layer split, in two named tiers (naming rules in
[`CODE_CONVENTIONS.md`](./CODE_CONVENTIONS.md)):

- **Raw hook** — wraps one `useQuery`/`useMutation`, returns the TanStack result verbatim; suffix
  `…Query` / `…Mutation`.
- **Facade hook** — composes raw hooks and returns a remapped **domain** object (`useConcert` →
  `{ concert, isLoading }`, `useApply` → `{ apply, canApply }`, `useStripeAccount`). Takes the plain
  domain name. This is where orchestration lives: `onSuccess` invalidations, buffer→request mapping,
  submit sequencing, window/message listeners, navigate-vs-dialog branching.

```ts
// CORRECT — orchestration in a facade hook; the component renders + calls it
function useInviteMember() {
  const mutation = useInviteMemberMutation();
  const submit = (buffer: InviteBuffer) => {
    const parsed = inviteMemberRequestSchema.safeParse(buffer);
    if (parsed.success) mutation.mutate(parsed.data);
    return parsed;                         // component renders parsed.error inline
  };
  return { submit, isPending: mutation.isPending };
}
```

### The anti-patterns this replaces — never do these

- **Mutation wiring inside a component.** `MembersPage`'s `InviteForm`/`MembersRoster` instantiate
  mutations and hold `handleSubmit`/`handleRoleChange` with inline `.mutate({…}, { onSuccess, onError })`;
  `OrganizationForm` builds the whole `UpdateOrganizationRequest` in `handleSubmit`. Move it to a
  facade hook; the component keeps only the controlled-input buffer and JSX.
- **Side-effect orchestration in a component.** `PayoutAccountSection` wiring a `window` `message`
  listener + `refetchStatus().then(...)` + branching + window-opening belongs in a hook.
- **Derivation + validation in a component.** `DeclareDoorRevenueButton` running `safeParse` and
  computing totals inline — hoist into the hook, render the result.

---

## One `xApi` per resource; one axios instance per backend service

The data layer has a fixed structure (naming in [`CODE_CONVENTIONS.md`](./CODE_CONVENTIONS.md); this
is the shape and the wiring). It mirrors the backend's "typed HTTP clients — Refit" pattern: a typed
contract per remote surface, auth attached once at the edge.

- **`features/<feature>/api/xApi.ts`** default-exports an object literal of `async` arrow methods that
  call the shared axios instance, type the response on the generic, destructure `{ data }`, return it.
  A `@b2b/*` api file that only re-exposes a shared one is a **pure re-export**, never a copy.
- **One axios singleton per backend the site calls** (`api`, `paymentApi`, `searchApi`, `customerApi`),
  created + configured (base URL, `qs` serializer) in `app/shared/src/lib/*Client.ts` with **no auth**
  — that layer can't know the site's identity.
- **Auth/interceptors attach in the app tree** (`web/shared/lib/axios.tsx`, `web/b2b/shared/lib/b2bAxios.ts`):
  OIDC bearer, B2B `X-Tenant-Id` (read from the identity store's active tenant), `removeUser()` on 401.
  *Which* backends a site may call, and with *what* token, is an app-level decision.

```ts
const organizationApi = {
  update: async (req: UpdateOrganizationRequest): Promise<Organization> => {
    const { data } = await api.put<Organization>("/organizations", req);
    return data;
  },
};
export default organizationApi;
```

### The anti-patterns this replaces — never do these

- **A second client for a backend that already has one.** One singleton per service; configure it, don't
  recreate it.
- **Auth wiring in `lib/*Client.ts`.** Bearer/tenant/401 belong in the app tree, not the shared
  factory. *(Standing tech debt: the four `*Client.ts` files and their per-app interceptor wiring are
  near-verbatim copies; the target is a `createApiClient(name)` factory + shared `attachAuth(client)` —
  logged in the nearest `TECH_DEBT.md`. Same two-layer shape, no duplication.)*
- **Manual `JsonDocument`-style ad-hoc fetching** where the `xApi` object expresses the call — the typed
  object is the readable source of truth.

---

## The write boundary is a zod parse: buffer → parsed → `XRequest`

Every user-editable form validates its controlled-input buffer against a **zod** schema at submit and
maps the **parsed** result — never the raw buffer — to the `XRequest`. This mirrors the backend's
`Validators/`: the client parse is a UX affordance (inline field errors, a real `isValid` gate), the
server re-validates every field regardless — the client is **not** the trust boundary.

- The schema lives in `features/<feature>/schemas/`. Keep it aligned to the request with
  `type XRequest = z.infer<typeof xRequestSchema>` so drift is a compile error.
- `safeParse` yields per-field messages the component renders next to each input, plus a derived
  `isValid` that gates submit — validity the server `400` could only report after a round trip.
- The parse **narrows the type at the boundary**: `parsed.data` is proven present and typed, so mapping
  to the `XRequest` needs no `!` bang and no `?? fallback`. The non-null assertion *is* the missing
  validation — the schema removes it honestly.

```ts
const parsed = updateConcertRequestSchema.safeParse(buffer);
if (!parsed.success) return parsed;        // component renders parsed.error.issues inline
updateConcert(parsed.data);                // parsed.data IS UpdateConcertRequest — no `draft!`
```

### The anti-patterns this replaces — never do these

- **Raw buffer → `XRequest` with a `!` bang or `?? fallback`.** The bang is the missing parse; a schema
  proves the fields instead of asserting past them (`useMyVenue`/`useMyArtist`, `OrganizationForm`).
- **A form with free-typed fields and no schema.** No `schemas/` folder for a feature that has editable
  inputs is the tell (`organizations`).
- **Client validation reported by `toast`** instead of inline from the parse result.

---

## Dispatch on a closed key with a table, not a scattered branch

The FE analog of the backend's **keyed strategy resolver**. When behaviour varies by a closed key —
a `$type` discriminator, a `TenantRole`, a `DealType` — resolve it through **one** table keyed on that
value, with a `never` exhaustiveness arm so a new backend member breaks the build. Never sprinkle the
same `switch`/ternary on the key across components and hooks.

- Backend polymorphism (`[JsonPolymorphic]`) → a TS discriminated union on `$type`
  ([`CODE_CONVENTIONS.md`](./CODE_CONVENTIONS.md)); dispatch via `Record<X["$type"], …>`.
- Role→permission is a single `hasPermission(role, permission)` over **one** matrix whose entries use
  the backend `SharedPermissions` constant **names** (`MembersInvite`, `MembersManageRoles`, …). One
  complete matrix, sourced from the backend's, not a hand-picked subset.

```ts
// CORRECT — one table, exhaustive; a new $type is a compile error
const render: Record<PaymentAmount["$type"], (p: PaymentAmount) => ReactNode> = {
  flat: (p) => …, doorShare: (p) => …, guaranteedDoor: (p) => …,
};
```

### The anti-patterns this replaces — never do these

- **A `switch`/ternary on `$type` or `role` inlined across components.** The rule ends up copy-pasted
  and drifts; keep it in one table.
- **A partial, hand-maintained permission matrix.** `tenantPermissions.byRole` modelling 4 of the 13
  backend permissions silently desyncs the gate the day the backend matrix changes. Model the full set,
  aligned to `SharedPermissions`, and treat the backend as the source (the FE gate is cosmetic; the
  server enforces).
- **Returning a label the caller must re-`switch`.** Resolve to the value, not an enum every consumer
  re-interprets.
