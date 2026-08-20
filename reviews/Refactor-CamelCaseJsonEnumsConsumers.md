# Code review — Refactor/CamelCaseJsonEnumsConsumers

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `d3112b99189d6e3c69931ec7c2c7f85969abc3f7`  _(2026-08-20)_
**Security-reviewed up to commit:** `d3112b99189d6e3c69931ec7c2c7f85969abc3f7`  _(2026-08-20)_ — no findings. The rework only moves the same strict camel-case converter to the three serialization seams and removes per-type attributes; deserialization targets known closed types (no polymorphic type-name binding from untrusted input) and the bus now rejects integer enum values (`allowIntegerValues:false`) — a net tightening. No auth/authz, routing, or input-parsing surface changed.

> Range reviewed: `836a15a56..cdf21ea2a` (2 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

> Commits after the `cdf21ea2a` review are structural only — `origin/main` merges (currency +
> platform-sync pins to 1017), the surface trim that split venue/artist into consumer PR #600, and a
> "keep-both" messaging merge conflict (label export + report feature). No new enum logic to re-review.

## Findings

- [x] **NAT1 — MEDIUM — correctness (missed asymmetry)** — `app/web/b2b/artist/src/features/dashboard/fixtures/empty.ts:17`
  `stripeConnect.state: "Incomplete"` not flipped when `StripeConnectState` became camelCase → invalid literal, breaks typecheck. Fixed → `"incomplete"` (the `empty.ts` fixtures were skipped by the mid/thriving-only fixture pass).
- [x] **NAT2 — MEDIUM — correctness (missed asymmetry)** — `app/web/b2b/venue/src/features/dashboard/fixtures/empty.ts:18`
  Same leftover `state: "Incomplete"`. Fixed → `"incomplete"`.

### Layers that found nothing

- **Layer 1 (native review), otherwise clean:** verified `StrictCamelCaseEnumConverter<T>` (camelCase + `allowIntegerValues:false` + `struct, Enum`; `dnB`/`hipHop` correct); `MessageAction`/`MessageSenderKind`/`HeaderType` keep explicit converters so the global policy can't regress them; `PaymentMethod`/`ApplicationStatus` are int columns, only JSON-serialized through the controller pipeline, so dropping the attribute leaks no integers; all label maps complete (GENRE 8/8, MESSAGE_ACTION 3/3, TENANT_ROLE 6/6, PAYMENT_METHOD 2/2); all comparison sites flipped in lockstep.
- **Security layer:** no findings (see marker above).
- **Lens B microservice isolation:** clean — the new converter lives in the shared `Concertable.Contracts` package; no data service gained a reference to another's non-Contracts project; no new `WaitFor`.
- **Lens C module boundaries / Lens D seeding:** clean — no cross-module calls or seeder writes changed.
- **Lens E conventions:** clean — no comments added; label maps follow the existing `GENRE_LABELS`/`DEAL_TYPE_LABELS` convention; null-vs-undefined unaffected.
- **Lens F test coverage:** Genre + MessageAction boundary tests added; `MessageSenderKind` covered by `MessagingInboxTests` (updated); global-policy enums covered by the existing `AddApplicationJson` mechanism test.
- **Authz check:** backend keys `FrozenDictionary<TenantRole, …>` by enum value, not wire string — casing flip cannot bypass authorization.

## Incremental review — 2026-08-20 (seam rework)

> Range: `d1422b6b..7306a8261` (full PR, re-reviewed after the approach change). The per-enum
> `[JsonConverter]`/`[JsonStringEnumMemberName]` attributes were replaced by one strict camel-case
> converter applied at the three serialization seams (MVC `AddApplicationJson`, SignalR
> `AddSignalR().AddJsonProtocol`, bus `MessageSerializer`); all per-enum attributes and the bespoke
> `StrictCamelCaseEnumConverter` were deleted. Native (Layer 1) + security (Layer 2) + architecture lenses.

- **Layer 1 (native), clean in-range:** confirmed all four hosts route MVC through `AddApplicationJson`,
  the single `AddSignalR` registration carries the converter, and `MessageSerializer` is the only other
  serialization path touching these enums. `JsonNamingPolicy.CamelCase` reproduces every wire value
  exactly (`DnB`→`dnB`, `HipHop`→`hipHop`, `FlatFee`→`flatFee`, `Org`→`org`); `DealTypeNames`/
  `HeaderTypeNames` survive as `[JsonDerivedType]` `$type` discriminators, unaffected by the enum policy.
- **Lens F:** MVC seam covered by `GenreWireFormatTests`; bus seam covered by the new
  `MessageSerializer` enum tests (camel-case out, integer rejected); `MessageAction` boundary test
  rerouted through the shared policy. SignalR seam has no unit test but the merge-queue mailbox/
  notification UI E2E exercises it end-to-end (a PascalCase regression breaks the camel-case label
  mapping and fails the scenario), so it is covered, not a gap.
- **Lenses B/C/D/E:** clean — converter registrations are inlined at each seam (no new cross-service or
  cross-module reference; `MessageSerializer` in shared messaging infra, SignalR in shared notification);
  no seeders, no logging templates, no comments, style-conformant.

### Out of scope — deferred consumer surfaces (NOT this PR's diff)

The native pass flagged PascalCase literals in `app/web/b2b/{venue,artist}/**`
(`VenueAcceptCheckoutPage.tsx`, `ApplicationCard.tsx`, dashboard `mid/thriving` fixtures). These files
are **not modified by this PR** and are identical to `origin/main`: commit `2c00f7bbc` deliberately keeps
the venue/artist surfaces on the published-package PascalCase baseline because they carve-build against
the published `@concertable/*` packages, which only republish after this producer PR merges. Their
internal consistency is gated by the `carve-fe (web/b2b/{venue,artist})` checks (green on the producer
diff). Migrating them to camel-case is the stacked consumer cut-over (`Refactor/CamelCaseJsonEnumSurfaces`),
legal only once the new package is on the feed — i.e. after this merge + platform sync.
