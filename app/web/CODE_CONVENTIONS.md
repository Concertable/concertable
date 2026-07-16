# Frontend Code Conventions

The web/mobile counterpart to [`api/docs/CODE_CONVENTIONS.md`](../../api/docs/CODE_CONVENTIONS.md).
Same voice: rule first, one-line rationale, a litmus test where it helps.

**These rules are set by what is idiomatic for a hand-written TypeScript/React client against an
ASP.NET Core backend — not by what the current `app/` code happens to do.** The code is evidence of
where we *conform* or *violate*, never the justification. Where a rule is contested, the source is
linked inline. Where existing code breaks a rule, it's flagged as a **violation to fix**.

The idea the whole file rests on: **a TypeScript type identifier is local; the JSON contract is
not.** What must match the backend byte-for-byte is *field names and their casing*. The type's *name*
never travels on the wire — so the frontend names types for its own domain and its own viewpoint.

---

## Reads are named for the domain noun — no `Dto`, no `Response`

A type that mirrors a backend **read** is named for the *thing*: `Application`, `Concert`,
`Checkout`, `Opportunity`, `Contract`. No suffix.

`Dto` and `Response` are the *server's* words. `Dto` is pure backend layering (it keeps C# services
callable off the HTTP layer — see the backend doc); it never had meaning on the client. `Response`
describes what the server *sends* from the server's viewpoint — but to the client that payload is
just a `Concert`. Those suffixes are idiomatic **only in machine-generated clients**, where NSwag /
openapi-typescript mirror the C# schema names verbatim. We hand-write our client, so we drop them —
the prevailing convention for hand-written React is the domain noun.

**Litmus:** *rename this TS type — does any JSON byte change? No → the name is yours; name it for the
domain.*

> **Violations (fix):** `PaymentResponse` (`shared/.../concerts/types.ts:78`) and the
> structurally-identical `TicketPurchaseResponse` (`customer/.../tickets/api/ticketApi.ts:10`). Both
> are payment *outcomes*. Collapse to one shared `PaymentOutcome` and delete the duplicate.

## Write inputs are `XRequest` — carrying only client-settable fields

The body POSTed/PUT to a write endpoint — and passed as the TanStack `useMutation` variables — is
`XRequest`: `CreateArtistRequest`, `UpdateConcertRequest`, `ESignatureRequest`.

This is not a leaked server word, and that's the whole point of keeping it when we dropped `Response`:

- `Response` is the server's word for what the FE *receives*. `Request` is **the client's own
  accurate word** — the frontend is literally constructing an HTTP request to send. It describes what
  the FE code is doing from *its* viewpoint.
- It mirrors the backend's `Module.Application/Requests/` records 1:1, so **one grep-able name spans
  the whole stack** (`CreateArtistRequest` in C# and in `applicationApi.ts`), and nothing needs
  renaming if we ever adopt codegen (which would emit exactly this name).

TanStack itself only names the *slot* — the `useMutation<TData, TError, TVariables>` generic — never
a type suffix ([TanStack TS docs](https://tanstack.com/query/latest/docs/framework/react/typescript)).
`Input` is a real alternative but is GraphQL-native (it exists there to dodge a namespace collision
that doesn't exist in REST/TS); adopting it would add a gratuitous `Request`→`Input` translation when
the backend already says `Request`.

Same field discipline as the backend request records:

```ts
// CORRECT — client's half of the contract; server stamps user/time/IP
export interface ESignatureRequest {
  signatoryName: string;
  drawnSignatureImage?: string;
}
```

- **Default optional/absent values to `?: T` (undefined); reach for `null` only when "deliberately set to
  empty" is a distinct, acted-on state from "never set."** `undefined` = unset/absent; `null` = an explicit,
  intentional "no value." Most fields carry only one absence meaning, so use `undefined`: it composes with
  optional params, optional chaining, and defaults, and `JSON.stringify` drops it so request bodies stay
  clean (an omitted key deserializes to the backend's `null`/absent). Don't type a field `| null` merely to
  mirror a backend `string?` — the API may serialize an explicit `null`, but the frontend contract stays
  `undefined`, and consumers read defensively (`?? fallback`, `!= null`) so a wire `null` reads the same as
  absent. Introduce `null` only when the two states genuinely differ and something downstream branches on it.
- Route/resource identity is a **function argument, never a body field**:
  `applyToOpportunity(opportunityId, eSignature)`. Identity comes from the route, not the body.
- Share one `XRequest` when create and update take the identical writable shape; split into
  `CreateXRequest` / `UpdateXRequest` the moment they diverge.

**Litmus:** *could the client legitimately set this field on the way in? No (it's a route id, or
server-owned) → it's not in the `Request`.*

> **Violations (fix):** `CreateArtist` and (partially) `OpportunityDraft` don't carry the suffix while
> `CreateReviewRequest` / `CreatePreferenceRequest` / `TicketPurchaseRequest` do. Rename `CreateArtist`
> → `CreateArtistRequest`. Keep `OpportunityDraft` — `Draft` is a genuine domain noun (the pre-publish
> shape), not a stand-in for `Request`.

## Contract types live in the feature's `types.ts` — reads and `XRequest`s alike

Every feature owns one `features/<feature>/types.ts` holding the domain reads *and* the `XRequest`
inputs, exported. `api/xApi.ts` **imports** them; it never declares its own. Feature-folder
colocation of types is the standard React structure, and one file answers "what does this feature
exchange with the backend."

Name the type (don't inline the object) once it crosses a function boundary — i.e. it's the declared
`useMutation` variables type, or it's shared by the api fn and the hook/component. Inline anonymous
object types only for a truly single-use, one-or-two-field body at one call site.

**Litmus:** *to see everything this feature exchanges with the server, is one file enough?*

> **Violations (fix):** `ESignatureRequest` (`applicationApi.ts:7`), `UpdateConcertRequest`
> (`concertApi.ts:4`), `TicketPurchaseRequest` (`ticketApi.ts`), `CreateReviewRequest` (`reviewApi.ts`)
> are declared inline in the `api/` file. Move to the feature `types.ts`.

**We hand-write these types — no codegen**, by choice: it lets the FE name for its own domain and
viewpoint (the rules above). The cost is silent drift from the backend, accepted while the surface is
small and co-owned. If we ever adopt codegen, generate *types-only*
([openapi-typescript](https://openapi-ts.dev/introduction)) into a quarantined module and re-export
domain-named aliases — never let generated `XResponse`/`XDto` names into feature code.

## Casing — camelCase everywhere; no mapping step

TS fields are **camelCase** and match the JSON key-for-key. ASP.NET Core's web defaults serialize
camelCase ([Microsoft docs](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/configure-options#web-defaults-for-jsonserializeroptions)),
so the wire is camelCase out of the box and **no client-side case conversion exists or is wanted** —
the axios layer transforms only query-string params.

- A PascalCase field in JSON is a *backend bug* (someone serialized with raw `System.Text.Json`
  outside the MVC/Minimal-API pipeline, which preserves C# casing). Fix it there — never add a FE
  key-rewriter.
- **The one PascalCase exception is `FormData`.** Multipart field names bind to C# by property name,
  not through the JSON policy, so uploads use `"Name"`, `"Banner"`, `"Genres[0]"` (`artistApi.ts`).
  Correct, and stays.

**Litmus:** *JSON body → camelCase. `FormData` field name → PascalCase (it's a C# property binder,
not JSON).*

## Object shapes are `interface`; unions/aliases/derived types are `type`

Interfaces for object shapes, `type` for everything that isn't a plain object — the common TS style
guidance, and it keeps the two roles visually distinct.

- `export interface` for every object/record shape.
- `type` for: string-literal unions (`ApplicationStatus`), discriminated-union umbrellas
  (`PaymentAmount`, `Contract`, `User`), template-literal types (`SortToken`), utility-derived types
  (`UserRole = Exclude<Role, "Admin">`).
- Extend with `interface X extends Y` (`Opportunity extends OpportunityDraft`), not intersections.

## Polymorphic JSON → discriminated union on `$type`

Backend polymorphism (`[JsonPolymorphic]` / `[JsonDerivedType]`) emits a `$type` discriminator by
default ([Microsoft docs](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism)).
Model it as a TS [discriminated union](https://www.typescriptlang.org/docs/handbook/2/narrowing.html#discriminated-unions)
keyed on `$type`, with **camelCase literal values matching the backend's `[JsonDerivedType]`
discriminators** — that key and those values are part of the wire contract, not a naming choice.

```ts
export interface FlatPayment { $type: "flat"; amount: number; }
export interface DoorSharePayment { $type: "doorShare"; artistPercent: number; }
export type PaymentAmount = FlatPayment | DoorSharePayment | GuaranteedDoorPayment;
```

- Narrow with `switch (x.$type)`; dispatch tables key off `Record<X["$type"], …>`. Add a `never`
  exhaustiveness arm so a new backend subtype breaks the build, not runtime.

**Litmus:** *does the backend send more than one shape under one field? → discriminated union on
`$type`, values copied from `[JsonDerivedType]`.*

> **Violation (resolve):** `User` carries two discriminants — `$type` (camelCase, polymorphism) and
> `role` (PascalCase, used by the `isVenueManager` guards). Pick one narrowing key.

## Response typing — put the shape on the axios generic

Type the call at `api.get<T>` / `api.post<T>` and return `data`; the generic flows to
`AxiosResponse<T>.data` ([axios TS](https://github.com/axios/axios#typescript)).

```ts
applyToOpportunity: async (opportunityId: number, eSignature: ESignatureRequest): Promise<Application> => {
  const { data } = await api.post<Application>(`/application/${opportunityId}`, { eSignature });
  return data;
},
```

Bodyless endpoints use bare `api.post(url)`, no generic. Binary reads pass
`{ responseType: "arraybuffer" }` and wrap in a `Blob` (`getContractPdf`).

## Errors are `ProblemDetails`, handled once at the query client

The error body is ASP.NET Core `ProblemDetails` ([RFC 9457](https://datatracker.ietf.org/doc/html/rfc9457)):
`{ title, detail, errors[] }`. It is handled **centrally** in `QueryCache` / `MutationCache`
`onError` (`queryClient.ts`) — the [TkDodo-recommended](https://tkdodo.eu/blog/react-query-error-handling)
place for global error UI. Feature hooks and components **do not** `try/catch` to toast errors — the
client does.

- Opt out per call with typed `meta`: `silenceErrors`, or `expectedErrors: [404]` for a status the
  caller handles itself (registered via TanStack module augmentation).
- Shared retry policy (`queryRetry.ts`): network errors + transient statuses
  (`408/429/502/503/504`) only, max 2×.
- **Allowed exception:** a route guard may `isAxiosError(e) && status === 401` to `throw redirect(...)`
  (venue/artist `guards.ts`) — that's control flow, not error reporting.

**Litmus:** *writing a `catch` to `toast` an API error? Stop — the query client already did. Only
catch to change control flow (redirect, fallback).*

> **Violation (fix):** `ProblemDetails` is private to `web/shared/lib/queryClient.ts` and
> re-implemented on mobile. Lift it and the `handleError` policy into `@concertable/shared`.

## API clients — one `xApi` object per resource, in `features/<feature>/api/`

A typed API module kept out of components is the standard React data-layer split. Our spelling: an
`api/xApi.ts` per resource, default-exporting an object literal of `async` arrow methods that call
the shared axios instance, destructure `{ data }`, and return it.

```ts
const applicationApi = {
  applyCheckout: async (opportunityId: number): Promise<Checkout> => {
    const { data } = await api.post<Checkout>(`/application/opportunity/${opportunityId}/checkout`);
    return data;
  },
};
export default applicationApi;
```

A `@b2b/*` api file that only re-exposes a shared one is a pure re-export
(`export { default } from "@concertable/shared/features/concerts/api/applicationApi"`), not a copy.

## One axios instance per backend service; configure in `lib`, intercept per app

The multi-service backend (own-site, Payment, Search) forces one axios singleton **per backend the
site calls** — `api`, `paymentApi`, `searchApi`, `customerApi` — each created in
`app/shared/src/lib/*Client.ts` with the shared `qs` param serializer and a
`configure<X>Api(baseURL)` setter.

Two layers, because *which* backends a site may call and with *what* token is an app-level decision
(see [`app/web/shared/CLAUDE.md`](./shared/CLAUDE.md) and [`app/web/b2b/shared/CLAUDE.md`](./b2b/shared/CLAUDE.md)):

- **`lib/*Client.ts` (shared):** creates + configures the instance. No auth, no interceptors — it
  can't know the site's identity.
- **App tree (`web/shared/lib/axios.tsx`, `web/b2b/shared/lib/b2bAxios.ts`):** side-effect-imports
  the singleton and attaches interceptors — OIDC bearer, B2B `X-Tenant-Id`, `removeUser()` on 401.

**Litmus:** *does this touch the user's token or tenant? → interceptor wiring, in the app tree, not
`lib/*Client.ts`.*

> **Tech debt (not a rule):** the four `*Client.ts` files are near-verbatim copies and per-app
> interceptor wiring is copy-pasted (only `b2bAxios.ts` factors it into `attach()`). A
> `createApiClient(name)` factory + shared `attachAuth(client)` removes the duplication without
> changing the two-layer shape. Log in the nearest `TECH_DEBT.md`.

## TanStack Query — raw hooks carry `Query`/`Mutation`; facades take the domain name

Wrapping `useQuery`/`useMutation` in a per-feature custom hook is the
[TanStack community pattern](https://tanstack.com/query/latest/docs/framework/react/guides/mutations);
components never call `useQuery` directly. Two tiers, named by what they return:

- **Raw hook** — returns the TanStack result verbatim (`.data`, `.isPending`, `.mutate`). The suffix
  is **mandatory** and states the kind: `useConcertQuery`, `useAcceptApplicationMutation`. A bare
  name on a raw hook is the violation.
- **Facade hook** — composes one or more raw hooks and returns a remapped domain object
  (`useConcert` → `{ concert, isLoading }`, `useReviews`, `useApply` → `{ apply, canApply }`). It
  takes the plain domain name *because* it's no longer a raw query — it's the app-facing API. This
  is the idiomatic "abstract TanStack away" hook, not a naming lapse.

Non-data hooks (`useDebounce`, `useIsMobile`, `useRole`) are neither — they never take a suffix.

Hooks live in `features/<feature>/hooks/`, one concern per file.

**Litmus:** *does this hook hand back the raw TanStack object? → `…Query`/`…Mutation`. Does it hand
back a domain shape? → plain `useX`.*

> **Violations (fix):** raw `useQuery`/`useMutation` wrappers with bare names — the dashboard hooks
> (`useVenueKpis`, `useArtistOverview`, … ~20 of them) and `useStripeAccount` return the raw query
> yet lack the suffix. Rename to `useVenueKpisQuery` etc. The facades (`useConcert`, `useReviews`,
> `useApply`, `useMyVenue`, …) are already correct — leave them.

## Query keys — arrays, generic → specific, per-feature factory

Keys are arrays ordered most-generic → most-specific, resource name first:
`["applications", "opportunity", opportunityId]`; invalidate by prefix. Centralize a feature's keys
in one exported factory object ([TkDodo, *Effective React Query Keys*](https://tkdodo.eu/blog/effective-react-query-keys)),
so a key and its invalidations can't drift apart across files.

> **Violation (adopt incrementally):** keys are inline array literals everywhere, no factory.
> Introduce the factory per feature as each is touched.

## Mutation variables vs form state — buffer is the component's, payload is the mutation's

- The **live controlled-input buffer** is local `useState` in the component. It is *not* an
  `XRequest`, and it *never* holds server data copied out of the query cache — copying breaks
  background refetch ([TkDodo, *React Query and Forms*](https://tkdodo.eu/blog/react-query-and-forms)).
- On submit, the component maps its buffer to the `XRequest` and passes it as the mutation
  **variables** to `mutate(request)`.
- The `useXMutation` hook **binds everything constant** for its lifetime (route ids, `onSuccess`
  invalidations) and takes only the per-submit variables. Don't thread a fixed `opportunityId`
  through `mutate()` if the hook already closed over it.

**Litmus:** *changes per submit → mutation variable, passed to `mutate()`. Fixed for the hook's life
→ bound inside `useMutation`.*

## Form buffers are validated by a zod schema before becoming an `XRequest`

Every user-editable form validates its buffer against a **zod** schema at submit and maps the
*parsed* result — never the raw buffer — to the `XRequest`. zod is already the project's validation
tool (TanStack `validateSearch`; `SearchSchema` in `features/search/schemas/`), so this adds no
dependency. A form with free-typed fields and no schema is the violation.

One schema does two jobs a hand-rolled `if` can't do together:

- **It narrows the type at the boundary.** `schema.parse(buffer)` returns a value whose fields are
  proven present and correctly typed, so mapping to the `XRequest` needs no `draft!` bang and no `??`
  fallback. The non-null assertion *is* the missing validation — a schema removes it honestly instead
  of asserting past it.
- **It feeds inline field errors.** `safeParse` yields per-field messages the component renders next
  to each input, plus a derived `isValid` that gates the submit button. React state then reflects
  *actual* validity, not a guess — the UX a server `400` can only deliver after a round trip.

The schema lives in `features/<feature>/schemas/`, matching the search precedent. Keep it aligned with
the `XRequest` shape — `type XRequest = z.infer<typeof xRequestSchema>` makes drift a compile error —
while the naming and casing rules above still hold (camelCase fields matching the wire).

Client validation is a UX affordance, **not** a trust boundary: the server re-validates every field
regardless (backend `Validators/`). Never drop a server check because the client has one.

**Litmus:** *a field the user can type into, with nothing parsing it before `mutate()`? → add the
schema; the parsed output, not the buffer, becomes the `XRequest`.*

> **Reference implementations:** the concert edit form (`useMyConcert.ts` + `updateConcertRequestSchema`,
> which parses the draft and kills the old `draft!` bang) and the apply/accept signature
> (`eSignatureRequestSchema` + `useESignature`, with the per-field message in `ESignaturePanel`).
> **Adopt incrementally:** the venue/artist edit forms (`useMyVenue`/`useMyArtist`) and the remaining
> write inputs (`CreateArtistRequest`, `CreateReviewRequest`, `CreatePreferenceRequest`,
> `TicketPurchaseRequest`) still map buffers unchecked — add a schema as each is touched.

---

## Violations at a glance

| # | Rule broken | Location | Fix |
|---|---|---|---|
| 1 | No `Response` on a read | `concerts/types.ts:78`, `tickets/api/ticketApi.ts:10` | Collapse to one shared `PaymentOutcome` |
| 2 | `XRequest` suffix on write inputs | `artists/api/artistApi.ts` | Rename `CreateArtist` → `CreateArtistRequest` |
| 3 | Contract types live in feature `types.ts` | `applicationApi.ts:7`, `concertApi.ts:4`, `ticketApi.ts`, `reviewApi.ts` | Move inline `XRequest`s out of `api/` |
| 4 | Single `$type` discriminant | `auth/types.ts` (`User`) | Pick one narrowing key |
| 5 | Shared `ProblemDetails` | `web/shared/lib/queryClient.ts` | Lift into `@concertable/shared` |
| 6 | Suffix on *raw* query/mutation hooks | dashboard hooks (~20), `useStripeAccount` | Rename `useVenueKpis` → `useVenueKpisQuery`; leave facades |
| 7 | Per-feature query-key factory | everywhere | Adopt incrementally |
| 8 | *(tech debt)* duplicated axios clients | `shared/lib`, app trees | Factory + `attachAuth`; log in `TECH_DEBT.md` |
| 9 | Form buffer unvalidated before `XRequest` | `useMyVenue`/`useMyArtist`, create artist/review/preference/ticket forms | zod schema in `schemas/`; map the parsed result (concert edit + e-signature done) |
