# Messaging group inbox — tenant-owned conversations

> **What this is:** the implementation detail for **USER_MODEL_PLAN Phase 8** (§7 "Messaging: the group
> inbox") and the messaging half of the `LAUNCH_PLAN.md:25` item *"Finish Swim-lane B … Phases 7-8
> remain."* Phase 6 (membership/invitations/UI) has shipped; the auth sweep (USER_MODEL_PLAN Phase 7) and
> this (Phase 8) are what's left. This plan owns Phase 8 end-to-end and supersedes the one-paragraph sketch
> in USER_MODEL_PLAN §7/§8 — read it alongside, not instead of, that plan.
>
> **Governance:** phases follow [`plans/AGENTS.md`](../AGENTS.md) — each independently shippable, each ends
> green, verification gate per phase (build + affected unit/integration always; E2E only when the phase is
> massive/risky, run via the merge queue not locally). Model-changing phases end with
> `./initial-migrations.ps1` from `api/` — never additive migrations.

---

## Does Phase 8 depend on Phase 7 (the auth sweep)? — **No hard dependency.**

Verified against code, not the plan prose. Phase 8 needs only **already-shipped** work:

- **Two-party tenant filter** (the Bucket-B OR-filter mechanics) — shipped in the tenant-scoping work
  (TS Phase 4). `ArtistReadModel`/`VenueReadModel` already carry `TenantId`
  (`api/Concertable.B2B/src/Modules/Concert/…/Domain/ReadModels/{Artist,Venue}ReadModel.cs:14`).
- **Membership** (`TenantMembershipEntity`) — shipped in USER_MODEL_PLAN Phase 1.
- **`MessagesRead` / `MessagesSend` permission catalog + `[HasPermission]`** — shipped in Phase 2
  (they are already in `PermissionCatalog` / §1.3 of USER_MODEL_PLAN; the `MessageController` just never
  applied them).
- **Active-tenant `X-Tenant-Id` resolution** (`ITenantContext`) — shipped in Phase 4.

Phase 7 retires `UserEntity.Role`, the manager profile tables, and the `role`/`owner` claims. **None of
that is on Phase 8's critical path.** `SentByUserId` attribution reads `ICurrentUser.Id`, which survives
Phase 7 untouched. So Phase 8 can ship **before, after, or in parallel with** Phase 7. This matches
USER_MODEL_PLAN §9, which lists Phase 8's only hard blocker as "TS Phase 4" — not Phase 7.

**Two soft couplings to coordinate (not block on):**

1. **`ApplicationNotifier.cs`** (`…/Concert.Infrastructure/Services/ApplicationNotifier.cs`) is edited by
   both sweeps — Phase 7 would re-point its `IUserModule.GetManagerByIdAsync` lookup; Phase 8 rewrites its
   addressing to tenant ids. Whichever lands second rebases. **Phase 8 owns the messaging-addressing rewrite
   of this file**; if Phase 7 lands first it should leave the messaging calls alone.
2. **`IUser` / `MessageMappers.ToMessageUser(this IUser user)`** — Phase 7 wants to "dissolve `IUser` if the
   user DTOs were its only consumers", but Conversations' mapper is *also* a consumer. **Phase 8 removes that
   consumer** (sender identity moves to org/member resolution, §Phase 2 below), which *shrinks* Phase 7's
   surface. So running Phase 8 first makes Phase 7 cleaner, never the reverse.

**Conclusion: no wait required.** Sequence by convenience; coordinate the two files above.

---

## No clash with `PLATFORM_COMMISSION.md`

Confirmed by reading both. The commission plan lives entirely in the **Payment** service (escrow/settlement
charge sites, `PlatformFeeOptions`, `EscrowEntity`/`SettlementTransactionEntity` snapshots) plus a
pricing-disclosure surface in the manager SPAs. It never touches the `Conversations` module, `MessageEntity`,
`MessageController`, or messaging notifications. The only shared *area* is "the manager SPAs" — but different
features (pricing breakdown at Apply/Accept vs. the Mailbox popover), no file overlap. Safe to run in parallel.

---

## 1. Current state (verified in code)

### 1.1 Backend — module `Concertable.B2B.Conversations`

Messaging is a B2B module at `api/Concertable.B2B/src/Modules/Conversations/`. It is **keyed to individual
users, not tenants** — this is the whole crux of the change.

- **`MessageEntity`** (`…/Conversations.Domain/Entities/MessageEntity.cs`) — `int` PK; fields
  `Content`, **`FromUserId` (Guid)**, **`ToUserId` (Guid)**, `Action? (MessageAction)`, `SentDate`,
  **`Read` (bool)**. No thread/conversation entity — a "thread" is implicit in the (from,to) user pair.
  `Read` is a **single global boolean on the message row** — there is no per-recipient read state.
- **`MessageEntityConfiguration`** — table `conversations.Messages`; indexes on `ToUserId`, `FromUserId`.
- **Inbox query** (`MessageRepository`) — `GetByUserIdAsync` returns messages where `ToUserId == id`
  ordered by `SentDate` desc; `GetUnreadCountByUserIdAsync` counts `ToUserId == id && !Read`;
  `MarkAsReadAsync(ids)` flips the boolean on specific message rows.
- **`MessageService`** — `GetForUserAsync` / `GetSummaryForUser` / `GetUnreadCountForUserAsync` all key off
  `currentUser.GetId()` (the recipient). Sender identity is resolved **cross-module** via
  `IUserModule.GetByIdAsync` / `GetByIdsAsync` → `MessageUser { Id, Email, Latitude, Longitude, County,
  Town }`. So the inbox shows the **sender's email + location**.
- **`MessageController`** (`…/Conversations.Api`) — routes `GET api/Message/user/summary`,
  `GET api/Message/user`, `GET api/Message/user/unread-count`, `POST api/Message/mark-read`.
  **No authorization attributes today** (§7 of USER_MODEL_PLAN notes this).
- **Cross-module facade `IConversationsModule`** — `SendAsync(fromUserId, toUserId, content, action)` and
  `SendAndNotifyAsync(…)`. The **only in-app producer** is the Concert module:
  `ApplicationNotifier` → `Messenger` → `IConversationsModule`. `ApplicationNotifier` resolves recipient
  **user** ids today (`GetVenueManagerIdAsync` selects `Venue.UserId`; `NotifyArtistAsync` uses
  `venue.UserId` / `artist.UserId`) — but both read models already carry `TenantId`, so re-pointing to
  tenant ids is a local change.
- **Notification** — `ConversationsNotifier.MessageReceivedAsync(userId, payload)` →
  `INotificationClient.SendAsync(userId, "MessageReceived", payload)` →
  `SignalRNotificationClient` → `hubContext.Clients.Group(userId)`. **`NotificationHub.OnConnectedAsync`
  adds each connection to a SignalR group named by its own user id.** So fanning out to all members is just
  "call `SendAsync` once per member user id" — **no hub change needed**.
- **Seeders** — `ConversationsDevSeeder` / `ConversationsTestSeeder` insert messages directly between
  `ArtistManager`/`VenueManager` **user** ids. (Conversation messages are a legitimate direct-seed: prod
  writes them through a normal service path, not an event handler — distinct from the never-seed list in
  `SEEDING_CONVENTIONS.md`, which is about the transport inbox/outbox.)
- **No test project exists for the Conversations module** — verified: it is absent from
  `Concertable.B2B.slnx` and there is no `Tests/` folder under the module. Messaging is exercised only
  *indirectly* through the Concert integration suite (via `ApplicationNotifier`). **USER_MODEL_PLAN §8's
  "update the Conversations integration suite" presumes a suite that does not exist — this plan must create
  it.**

### 1.2 Frontend — one shared module, three SPAs

- **Canonical implementation:** `app/shared/src/features/messaging/` (published as `@concertable/shared`).
  `app/web/shared/src/features/messaging/` is a thin re-export shim that additionally owns the one UI
  component, `Mailbox.tsx`.
- **`Mailbox.tsx`** (the inbox popover) renders in **all three** web SPAs (customer/venue/artist) via
  `Navbar` → `AppLayout`, shown only when logged in. **Sender is displayed as `message.fromUser.email`**
  (line 62) — email only. Action tag + content below it.
- **Types** (`app/shared/src/features/messaging/types.ts`) — `Message { id, fromUser: MessageSender, action?,
  content }`, `MessageSender { id: string, email: string }`. Sender is a **single user**.
- **API client** (`messageApi.ts`) — `GET /message/user/unread-count`, `GET /message/user` (paginated),
  `POST /message/mark-read {messageIds}`. **`/message/user/summary` has no frontend caller** (dead
  endpoint). **`markAsRead` mutation exists but has zero call sites** — the Mailbox never marks anything
  read; the only read signal the UI shows is the global `unreadCount` scalar.
- **Real-time** — SignalR `MessageReceived` subscribers exist in
  `useVenueNotifications.ts` / `useArtistNotifications.ts` but are **`console.log`-only** (no cache
  invalidation → a received message does **not** refresh the Mailbox). Customer doesn't subscribe to
  `MessageReceived` at all.
- **Not the messaging API (avoid confusion):** the dashboard `VenueInboxWidget`/`ArtistInboxWidget` render a
  `MessageThread {otherPartyName, unread, …}` preview from **hardcoded fixtures** (`dashboardApi.getInbox`),
  unconnected to `/message/user`. Their type already models "other party + unread boolean" — a useful shape
  reference, but mock data only.
- **Mobile** — `MessagesScreen.tsx` is a "coming soon" stub. Out of scope.

### 1.3 E2E

**There are no messaging E2E scenarios.** No `.feature` file and no `E2E_BASELINE.md` entry references
message/inbox/conversation/unread. (All "inbox"/"message" hits in the test tree are the service-bus
inbox/outbox pattern or SMTP `MailboxAddress` — unrelated.) The `Conversations` backend module *is* already
booted in the B2B E2E `AppFixture`, but nothing drives it from a scenario. So messaging E2E is **net-new**.

---

## 2. Target model (design decisions — defaults confirmed, trade-offs noted)

The provisional product-owner answers are adopted as the default; each is validated against the code and
the cheaper/safer option is noted where one exists.

1. **Tenant-owned conversation, visible to all members of the owning tenant** — *adopted*. `MessageEntity`
   becomes tenant-pair-scoped: **`FromTenantId`, `ToTenantId`, `SentByUserId`** (attribution). Thread
   identity = the tenant pair. Visibility = active tenant ∈ {`FromTenantId`, `ToTenantId`} (the TS-Phase-4
   two-party OR-filter) **and** the member holds `MessagesRead`. The code fully supports this today.

2. **Per-member read state over the shared thread** — *adopted*, as a **thread-level read pointer**
   `ThreadReadStateEntity { TenantId, UserId, CounterpartTenantId, LastReadAt }`. Unread for a member =
   messages in the thread with `SentDate > LastReadAt` that the member didn't send. Rejected alternative
   (per-message-per-user rows): row explosion, per USER_MODEL_PLAN §11. **Note:** the current UI never calls
   mark-read and shows only a global scalar, so per-member read is a genuine new behaviour — but it *degrades
   cleanly*: a single-member tenant has exactly one pointer, identical to today.

3. **Counterparty sees org identity; member attribution only within the owning tenant** — *adopted*.
   Rendering is computable server-side from the active tenant: a message where `FromTenantId ==
   activeTenant` (your org's outbound) shows the **member** who sent it (`SentByUserId`); a message where
   `ToTenantId == activeTenant` (inbound) shows the **counterparty org** identity only.
   - **Open design detail (→ §5 Q1):** *which* org identity — the tenant's registered `TenantDto.LegalName`
     (always present, from the Tenant module) or the Venue/Artist **profile `Name`** (the recognisable brand
     + the location the Mailbox shows today, via `IVenueModule`/`IArtistModule`, but absent until a profile
     exists). Recommendation: **profile `Name` + location, falling back to `LegalName`** — it preserves
     what's shown today and is the identity the counterparty actually recognises.

4. **New-message notifications to all members (v1, no assignment/claim)** — *adopted*. Add
   `ITenantModule.GetMemberUserIdsAsync(tenantId)` (a cross-module Contracts read; the inverse of the
   existing `GetMembershipsAsync(userId)`), optionally filtered to `MessagesRead` holders. `MessageService`
   fans the SignalR ping out per member id — no hub change. **Email fan-out:** the in-app email copy
   (`ApplicationNotifier`'s `EmailCopy`) currently targets one manager address; under the group inbox it
   fans to all members' emails. Notification *preferences* filtering is a named seam but there is **no
   member-preference store** yet, so v1 = all members holding `MessagesRead`.

5. **Backward compatibility (mandatory) + single-member degrade** — *satisfied by construction*. Every
   existing tenant has exactly **one** founding-Owner member, so active-tenant ≡ that user's tenant: the
   inbox lists the same threads, one read pointer ≡ today's read, one notification recipient ≡ today. The
   re-key is therefore **behaviour-preserving for the single-member (most artist) case** and additively
   correct for multi-member. See the migration note below — there is no data to preserve, so "compatibility"
   is about *behaviour*, not *data*.

### Migration note (no additive migration, no backfill)

This repo **re-scaffolds** — `./initial-migrations.ps1` from `api/` nukes and regenerates every context's
`InitialCreate`; there is no production data and every environment seeds from scratch. So the model change
carries **no data migration and no `FromUserId → FromTenantId` backfill** — the seeders are simply rewritten
to the tenant-pair shape and dev/test/E2E reseed fresh. Each model-changing phase below ends with a
re-scaffold as its final step.

---

## 3. Phases

Two phases. Phase 1 is a **behaviour-preserving backend cutover** (the risky data/model change, invisible on
the wire); Phase 2 is the **visible group-inbox UX + its net-new E2E**. Each is independently shippable and
ends green.

> **Why the split this way:** keeping Phase 1's `MessageDto` **wire-compatible** (still a `fromUser {id,
> email}` field, populated from `SentByUserId`'s identity exactly as today) means the live `Mailbox` and the
> whole existing app-wide E2E stay green while the *storage and addressing* become tenant-owned. The
> org-identity/attribution split and the UI that surfaces it then land together in Phase 2, so the DTO shape
> and its only consumer change in one shippable step — never a half-changed wire contract between phases.

### Phase 1 — Tenant-owned threads + per-member read + member fan-out *(backend; re-scaffold)*

**Model (`Conversations.Domain` + `Infrastructure/Data`)**
- `MessageEntity`: drop `FromUserId`/`ToUserId`/`Read`; add `FromTenantId`, `ToTenantId`, `SentByUserId`.
  `Create(fromTenantId, toTenantId, sentByUserId, content, sentDate, action?)`.
- New `ThreadReadStateEntity { Id, TenantId, UserId, CounterpartTenantId, LastReadAt }` + configuration
  (unique `(TenantId, UserId, CounterpartTenantId)`; index `(TenantId, UserId)`).
- `MessageEntityConfiguration`: indexes on `ToTenantId`, `FromTenantId`; register the new entity in
  `ConversationsDbContext` + `ConversationsConfigurationProvider`.

**Repository / service (`Infrastructure`)**
- Inbox re-scoped to the **active tenant** (`ITenantContext.TenantId`), thread visibility = active tenant ∈
  {`FromTenantId`, `ToTenantId`} via the TS-Phase-4 two-party OR-filter. Unread derived from the read
  pointer (`SentDate > LastReadAt` and `SentByUserId != me`), not a boolean. `mark-read` advances the
  pointer for `(activeTenant, currentUser, counterpartTenant)` rather than flipping message rows —
  `MarkMessagesReadRequest` gains the counterpart tenant (or becomes "mark thread read").
- **Sender identity stays wire-compatible in this phase**: `MessageDto.FromUser` is populated from
  `SentByUserId` (the sending member) exactly as the current mapper does — the org/attribution split is
  Phase 2. (`MessageMappers.ToMessageUser(IUser)` unchanged for now.)
- **Notification fan-out**: add `ITenantModule.GetMemberUserIdsAsync(tenantId)`; `SendAndNotifyAsync` pings
  each recipient-tenant member id via the existing `INotificationClient` (per-user SignalR group — no hub
  change).

**Cross-module facade + caller (`Contracts` + Concert module)**
- `IConversationsModule.Send/SendAndNotify` signatures change from `(fromUserId, toUserId, …)` to
  `(fromTenantId, toTenantId, sentByUserId, …)`. **In-repo cross-module facade, not a published-package
  contract** — Conversations is a B2B-internal module; no platform-sync gate (contrast the `Payment.Client`
  boundary rules in `plans/AGENTS.md` "Boundary-blocked refactors").
- `ApplicationNotifier` / `Messenger` re-pointed: use `venue.TenantId` / `artist.TenantId` (already on the
  read models) and `currentUser.GetId()` as `sentByUserId`; replace `GetVenueManagerIdAsync` (selects
  `Venue.UserId`) with a `GetVenueTenantIdAsync` (selects `Venue.TenantId`). Email copy fans to all member
  addresses (`GetEmailsByIdsAsync` over the member ids).

**Guards + seeders**
- Apply `[HasPermission(Permissions.MessagesRead)]` to the read endpoints and `[HasPermission(
  Permissions.MessagesSend)]` where posting applies, on `MessageController`.
- Rewrite `ConversationsDevSeeder` / `ConversationsTestSeeder` to the tenant-pair shape (seed the founding
  Owner as `SentByUserId`); read pointers are **never seeded** (they're per-member runtime state).

**Tests (new)**
- Create `Concertable.B2B.Conversations.UnitTests` + `Concertable.B2B.Conversations.IntegrationTests`
  (the module has none today) and add them to `Concertable.B2B.slnx`. Cover: two-party visibility (each side
  sees the thread; a third tenant sees nothing), per-member unread via the pointer, `MessagesRead`/`Send`
  gating (403 without the permission), fan-out to N members, and the single-member degrade (identical to
  today's counts).

**Gate:** `dotnet build api/Concertable.slnx` (0 errors) + new Conversations unit/integration + the Concert
integration suite (exercises `ApplicationNotifier`), via `integration-debug`. **Massive/risky** (re-keys a
covered flow, cross-module facade change, notification fan-out) → **let the merge queue run E2E; do not add a
skip trailer.** Ends with `./initial-migrations.ps1` from `api/`.

### Phase 2 — Org identity, member attribution, and the group-inbox UX *(backend DTO + frontend; net-new E2E)*

**Backend DTO (`Conversations.Application` + `Api`)**
- `MessageDto` gains the org-aware shape: the **counterparty org** identity (name + location) for inbound
  messages, and **member attribution** (`SentByUserId` → email/name) for your org's outbound messages —
  chosen server-side from the active tenant. Resolve org identity per §2.3 (profile `Name` via
  `IVenueModule`/`IArtistModule`, fallback `TenantDto.LegalName`); **this removes the `IUser` dependency in
  `MessageMappers`** (the Phase-7 soft coupling shrinks here).
- Delete the dead `GET api/Message/user/summary` endpoint if still unused (no frontend caller — verified),
  or keep only if a summary consumer is planned.

**Frontend (`app/shared/src/features/messaging/` + `app/web/shared`)**
- `types.ts`: `MessageSender` → an org-or-member sender shape (org name for counterparty; member for own).
- `Mailbox.tsx:62`: render the org name (counterparty) / member (own) instead of `fromUser.email`; wire the
  existing-but-unused `markAsRead` into open/scroll so the read pointer advances; unread badge is now
  per-active-tenant (drives correctly off the tenant switcher's `X-Tenant-Id`).
- `useVenueNotifications.ts` / `useArtistNotifications.ts`: make the `MessageReceived` handler
  **invalidate `["messages"]`** (today it only `console.log`s), so a received message refreshes the Mailbox.
- Optional/adjacent (flag, don't force): align the fixture-only dashboard inbox widgets to real tenant
  threads.

**E2E (net-new)**
- Add messaging `.feature` scenario(s) + page object(s): a venue member and an artist member exchange a
  message; a **second member** of the same tenant sees the same thread (the group-inbox proof) and read
  state is per-member. Append to `E2E_BASELINE.md` per its parser rules.

**Gate:** `dotnet build api/Concertable.slnx` + affected Conversations integration + **all web workspace
builds** (`pnpm`), via `integration-debug`. **Massive/risky** (changes a user-facing flow + new E2E) →
**UI E2E via the merge queue.**

---

## 4. Sequencing & interplay

- **Blocked by:** nothing outstanding — all prerequisites (TS-Phase-4 filter, Phase-1 membership, Phase-2
  permission catalog, Phase-4 active-tenant header) have shipped. **Not blocked by Phase 7** (see the top
  section).
- **Coordinate with Phase 7** on `ApplicationNotifier.cs` and the `IUser`/`ToMessageUser` consumer — Phase 8
  owns the messaging rewrite of both; running Phase 8 first makes Phase 7 strictly smaller.
- **Independent of `PLATFORM_COMMISSION.md`** (different service/files).
- **On completion:** tick USER_MODEL_PLAN Phase 8 + the `LAUNCH_PLAN.md:25` messaging clause in the same
  commit that lands Phase 2, and `git rm` this plan (plans/AGENTS.md Lifecycle 4).

## 5. Open questions for the product owner

1. **Which org identity does the counterparty see** — the Venue/Artist **profile brand name** (recommended;
   matches today's display + carries location) or the registered **`LegalName`**? Affects §2.3 and the Phase
   2 DTO. Default taken: profile name, fallback LegalName.
2. **Email fan-out volume** — v1 emails **every** member on a new message (no preferences store exists). Is
   all-members acceptable for launch, or should in-app-only be the default with email limited to Owners? (No
   assignment/claim mechanic in v1 either way.)
3. **Does a message ever need to target a specific member** (e.g. "@finance")? v1 assumes **no** — every
   message is org→org, visible to all members. Confirm that's fine for launch; a `ToUserId`-style narrowing
   would be a post-v1 additive change.
