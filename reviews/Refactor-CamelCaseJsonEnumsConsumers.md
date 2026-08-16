# Code review — Refactor/CamelCaseJsonEnumsConsumers

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `a927478f6`  _(2026-08-16)_
**Security-reviewed up to commit:** `a927478f6`  _(2026-08-16)_ — no findings (integers now rejected = net tightening; role/status mismatch fails closed, no authz bypass; published converter safe; test client is test-only).

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
