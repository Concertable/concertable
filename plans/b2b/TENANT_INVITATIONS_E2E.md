# Tenant Invitations — E2E test plan (release gate)

> Companion to [`TENANT_INVITATIONS_PHASE6.md`](./TENANT_INVITATIONS_PHASE6.md).
> **Hard rule: this feature does not release until every box under _Exit criteria_ is green.**
> Delete this file in the commit that lands the last of these scenarios in the suite
> (per [`plans/CLAUDE.md`](../CLAUDE.md) — plans are working docs, not an archive).

## Verdict — is this worthy of an E2E test?

Yes — but only the parts that actually cross the wire. Differentiated on purpose: E2E is reserved for
behaviourally-risky / cross-service flows (per `plans/CLAUDE.md`), not run by reflex where integration
already proves the behaviour.

| Surface | Layer | E2E-worthy? | Why |
|---|---|---|---|
| Member endpoints — list / change-role / remove / delete-org (6.2) | API | **No** — integration suffices | Synchronous, single-service, through the real auth pipeline; already covered (Tenant 34/34). E2E here only duplicates integration. |
| **Invited-registration** — register with an invited email → provisioning branch (6.3) | **API E2E** | **Yes** | Flips a core registration/provisioning path; async, event-driven, cross-service (Auth → ASB → B2B inbox → membership). Integration (in-process, single service) cannot exercise the real event propagation. |
| Already-registered user accepts via `POST /api/invitations/{id}/accept` (6.3) | **API E2E** | **Yes** | The second accept path; membership mint + idempotency under redelivery. |
| **Invite → accept → manage** end-user journey (6.4) | **UI E2E** | **Yes** | The feature *is* a human flow across pages + the tenant switcher; only a browser test proves it end to end. |

Net: **no new E2E for 6.2**; **API E2E is the 6.3 landing gate; UI E2E is the 6.4 landing gate.**

## API E2E (6.3 gate) — xUnit + Aspire full stack, via `e2e-api-debug`

Drive real HTTP against the booted stack; mint identities with `TestTokenMinter`; poll DB state until the
async outbox→inbox chain settles (don't assert synchronously).

1. **Invited-registration — happy path.** Owner A invites `newuser@…` as Manager
   (`POST /api/organizations/invitations`). A brand-new user registers on the manager client with that
   email → `CredentialRegisteredEvent`. **Assert:** the user becomes a **Manager of A's tenant**, gets
   **no personal tenant**, the invitation flips to `Accepted`, and Payment is **not** double-provisioned
   (inbox dedup on the `CredentialRegisteredEvent` MessageId holds).
2. **Accept by an existing account.** Invitee already has an account → `POST /api/invitations/{id}/accept`
   → membership created, invitation `Accepted`. Second call / redelivery is idempotent (no duplicate
   membership — `(TenantId, UserId)` unique index).
3. **Negatives.** Expired invite → rejected, no membership. Already-a-member → 409. Revoked invite cannot
   be accepted. Caller email ≠ invite email → rejected.

## UI E2E (6.4 gate) — Reqnroll + Playwright, via `e2e-ui-debug`

Append the scenario(s) to `api/Concertable.Shared/tests/Concertable.E2ETests/E2E_BASELINE.md` and the
matching `.feature`, then keep green via `e2e-ui-regress`.

1. **Invite → accept → manage.** Owner signs in → invites a member → invitee reaches the accept page via
   the emailed link and accepts → member appears in the roster → owner changes their role → owner removes
   them.
2. **Tenant switcher** surfaces the new membership for a user who belongs to more than one tenant, and
   switching stamps the `X-Tenant-Id` header so member-management acts on the chosen tenant.

## Exit criteria (release gate)

- [ ] API E2E scenarios 1–3 green in the API E2E suite (lands with **6.3**).
- [ ] UI E2E scenario(s) added to `E2E_BASELINE.md` and green via `e2e-ui-regress` (lands with **6.4**).
- [ ] Feature is **not** released until both boxes above are ticked.

> These cannot be written before the code they exercise exists — the invitation endpoints (6.3) and the
> members/accept UI (6.4). This doc is the standing requirement so that constraint never becomes an excuse
> to ship without them.
