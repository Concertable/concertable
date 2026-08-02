# Frontend Code Conventions

The web/mobile counterpart to [`api/agents/CODE_CONVENTIONS.md`](../../api/agents/CODE_CONVENTIONS.md).
Same voice: rule first, one-line rationale, a litmus test where it helps.

Sibling of [`CODE_PATTERNS.md`](./CODE_PATTERNS.md): this file is **naming and style**, that one is
**structure** (the tiers, identity composition, one-home-for-state, central error handling, the data
layer). A rule about *what to call a thing* lives here; a rule about *how the pieces fit* lives there.

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

> **Anti-pattern:** a read type named with a server suffix — `PaymentResponse`, `TicketPurchaseResponse`.
> A payment *outcome* is a domain noun: one shared `PaymentOutcome`. A ticket purchase is a
> `TicketPurchase`.

**The general rule — a suffix must distinguish two real shapes here.** The backend needs
Entity/Dto/Response because it genuinely holds three shapes of one concept. The frontend holds *one*
shape per concept: the thing it receives. So `Response` differentiates nothing and is pure noise.
Before keeping any suffix, ask what the *unsuffixed* name would collide with — if the answer is
"nothing", drop it.

That test is what keeps the suffixes we do use:

- **`XRequest`** — a real second shape, the write payload, sitting next to the read (`TicketPurchaseRequest`
  beside `TicketPurchase`). See the next section.
- **`GeocodeResponse`** — the raw Google Geocoding envelope (`{ status, results[] }`), a genuinely
  different shape from the `Coordinates` we hand back. `AxiosResponse` likewise is third-party.
- **`XPayload`** — event-carried data on a SignalR handler, a different axis from read/write.
  `TicketPurchasedPayload` earns it (the event shape differs from the HTTP `TicketPurchase` — its
  `ticketIds` are numbers, not strings); `ConcertDraftCreatedPayload = number` earns it (it names an
  otherwise-anonymous primitive). A **pure alias** of an already-domain-named type does not — the
  former `MessageReceivedPayload = Message` was deleted; handlers take a `Message`.

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

> **Note:** a write input carries the `XRequest` suffix (`CreateArtistRequest`) — but `Draft` in
> `OpportunityDraft` is a genuine domain noun (the pre-publish shape), not a stand-in for `Request`;
> keep it.

## Contract types live in the feature's `types.ts` — reads and `XRequest`s alike

Every feature owns one `features/<feature>/types.ts` holding the domain reads *and* the `XRequest`
inputs, exported. `api/xApi.ts` **imports** them; it never declares its own. Feature-folder
colocation of types is the standard React structure, and one file answers "what does this feature
exchange with the backend."

Name the type (don't inline the object) once it crosses a function boundary — i.e. it's the declared
`useMutation` variables type, or it's shared by the api fn and the hook/component. Inline anonymous
object types only for a truly single-use, one-or-two-field body at one call site.

**Litmus:** *to see everything this feature exchanges with the server, is one file enough?*

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
  (`PaymentAmount`, `Contract`), template-literal types (`SortToken`), utility-derived types
  (`WritableOpportunity = Omit<Opportunity, "id">`).
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

> **Resolution (decided):** the universal `User` is flat identity data with no `$type`, flat role,
> or product-specific subtypes. Product identity is composed in the owning tier
> ([`CODE_PATTERNS.md`](./CODE_PATTERNS.md), "Identity is composed, never widened").

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

The architectural shape of this — the single seam, the `meta` opt-outs, and the anti-patterns it
replaces — is [`CODE_PATTERNS.md`](./CODE_PATTERNS.md), "Errors are handled once." A feature-local
`onError` / `try-catch` toast fires *on top of* the central one: a confirmed **double-toast bug**, not
a style nit.

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

## One axios instance per backend service; create bare in core, enhance per app

The multi-service backend (own-site, Payment, Search) forces one axios instance **per backend the
site calls** — `apiClient`, `paymentClient`, `searchClient`, `customerClient` — each created bare in
`app/shared/src/lib/*Client.ts` (`@customer/shared` for `customerClient`). Only `searchClient` carries
the `qs` comma param serializer (it pairs with Search's `CommaDelimitedGenreArrayModelBinder`); the
other three send no array query params.

Two layers, because *which* backends a site may call and with *what* token is an app-level decision
(see [`app/web/shared/AGENTS.md`](../web/shared/AGENTS.md) and [`app/web/b2b/shared/AGENTS.md`](../web/b2b/shared/AGENTS.md)):

- **`lib/*Client.ts` (core):** `axios.create()` only — the bare instance. No baseURL, no auth, no
  interceptors: core can't know the site's identity.
- **App tree (`web/shared/lib/{apiClient,searchClient,paymentClient}.ts`, `web/b2b/shared/lib/b2bClient.ts`,
  and the mobile equivalents):** side-effect-imports the instance and enhances it through the shared fluent
  builder `configureClient(instance, url).withAuth(getToken, onUnauthorized).withTenant(getTenantId, header)`
  (`@concertable/shared/lib/client`). The interceptor bodies live once in the builder; each platform binds its
  auth flavour once — `configureWebClient` (OIDC `userManager`) / `configureMobileClient` (token storage) —
  and b2b chains `.withTenant(…, X-Tenant-Id)`.

**Litmus:** *does this touch the user's token or tenant? → the builder chain in the app tree, not
`lib/*Client.ts`.*

## TanStack Query owns all server state — never fetch or mutate from `useEffect`

Every server **read** is a `useQuery` and every server **write** is a `useMutation` (wrapped per the
naming rule below). This is the frontend counterpart of the backend's *"Refit, not hand-rolled
`HttpClient`"* ([`api/agents/CODE_PATTERNS.md`](../../api/agents/CODE_PATTERNS.md)): one sanctioned data
layer, never a bespoke one. **Do not** call an `api/xApi.ts` method from a `useEffect`, and never
hand-roll `useState` + `useEffect` + a promise to load or send server data.

TanStack already owns caching, request **dedup** (including React StrictMode's dev double-mount),
retries, routing errors to the central `QueryCache`/`MutationCache` handler, and `isPending`/`isError`
state. A `useEffect` that fires a request re-implements all of that by hand and worse — it double-fires
under StrictMode, drops the result when the component unmounts before the promise settles, and races on
out-of-order responses. Those are the exact bugs the library exists to remove.

This holds even for a **one-shot, fire-on-mount** action — e.g. accepting an invitation from an emailed
link. It is a `useQuery` (which fires on mount and dedupes by key), not
`useEffect(() => { api.accept(id).then(navigate) }, [])`. The success side-effects (set state, navigate)
run at the tail of the `queryFn`, not in a follow-up effect reacting to the result.

> **Anti-pattern:** `useEffect(() => { api.getX().then(setX) }, [])`, or an on-mount `mutate()` guarded
> by a `useRef` to dodge the StrictMode re-fire. Both are a hand-rolled reimplementation of `useQuery` —
> replace with the hook.

**Litmus:** *reading or writing server data? → a `useQuery`/`useMutation` hook. Reaching for `useEffect`
(or `useState`) to load or send it? → stop, that's the violation.*

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

Non-data hooks (`useDebounce`, `useIsMobile`) are neither — they never take a suffix.

Hooks live in `features/<feature>/hooks/`, one concern per file.

**Litmus:** *does this hook hand back the raw TanStack object? → `…Query`/`…Mutation`. Does it hand
back a domain shape? → plain `useX`.*

> **Note:** a raw `useQuery`/`useMutation` wrapper with a bare name is the violation (e.g. dashboard
> hooks that return the raw query but omit `…Query`). Facades (`useConcert`, `useReviews`, `useApply`,
> `useMyVenue`) correctly take the plain domain name — those are right, leave them.

## Query keys — arrays, generic → specific, per-feature factory

Keys are arrays ordered most-generic → most-specific, resource name first:
`["applications", "opportunity", opportunityId]`; invalidate by prefix. Centralize a feature's keys
in one exported factory object ([TkDodo, *Effective React Query Keys*](https://tkdodo.eu/blog/effective-react-query-keys)),
so a key and its invalidations can't drift apart across files.

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

## Zustand stores are private state owners; facade hooks are the feature API

A Zustand store is an implementation detail of the feature that owns the client state. Keep the
store module private to that feature: do not export it from the feature barrel, import it from a
component, or make consumers assemble behavior from selectors and actions. Components consume a
feature-facing facade hook that returns the domain values and actions they need.

- Store client state and state transitions only. TanStack Query remains the owner of server state;
  never mirror query data into Zustand.
- Put transitions in named store actions. A component must not call `setState`, and a public helper
  must not be a thin spelling of `store.getState().setX(...)`.
- Keep derivations pure by passing every input explicitly. A function that reads a store, query
  client, router, persistence, or browser global is infrastructure, not a pure domain function.
- Keep imperative access exceptional and cohesive. Route guards, request interceptors, logout, and
  similar non-React infrastructure use one internal feature service/session object; do not export a
  family of getter, setter, clear, and reconcile wrappers.
- Direct `getState()` and `setState()` access belongs only inside that internal boundary or focused
  store tests. React orchestration uses the facade hook and store selectors internally.

**Litmus:** *can a consumer import the store or call a standalone `getX`/`setX` wrapper? Yes: the
feature boundary is leaking; expose the domain operation from its facade hook or internal service.*

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
