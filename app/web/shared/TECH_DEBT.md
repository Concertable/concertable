# app/web/shared technical debt

## LOW

### `Navbar` grew boolean flags instead of decomposing, so a minimal consumer passes negations and a no-op

`Navbar.tsx` bundles four concerns behind one component: branding/links, `NavbarSearch`, `Mailbox`, and
`onHeightChange` (which only `AppLayout`'s `Breadcrumbs` spacing needs). Admin has none of the last three
-- it has no search, no messaging inbox, and no breadcrumbs bar -- so its only way to opt out is
`showSearch={false} showMailbox={false} onHeightChange={() => {}}` on every call site. `profileSlot` is
the one addition here that's the right shape (a real slot replacing `ProfileMenu`'s hardcoded
`/settings` links, per `tiered-shared-code`'s "shared code declares a slot, the app injects the
variation") -- `showSearch`/`showMailbox`/the forced `onHeightChange` are the anti-pattern that same rule
warns about: flags a consumer must actively negate, not a hole it fills.

Found adding the admin console's navbar (`app/web/admin/src/routes/_admin/route.tsx`): it needed the
branded shell (logo, `bg-primary` bar, nav links) but none of search/mailbox/height-tracking, and had no
way to ask for just that.

**Resolves when:** split `Navbar` into a minimal shell (logo + `links` + an end slot) that `AppLayout`
composes into the full version (adding `NavbarSearch`, `Mailbox`, and `onHeightChange` wiring itself),
so a bare consumer imports the shell directly instead of the full component with three concerns switched
off. `showSearch`/`showMailbox`/`onHeightChange` fall away entirely once nothing needs to negate them.

**Progress — PR1 (`Chore/TechDebtNavbarSlots`) landed the additive half:** `Navbar` gained an `endSlot`
prop and `onHeightChange` became optional — both purely additive, so every existing caller (admin,
`AppLayout`) is unaffected until the published `@concertable/web` alpha carries them (the `carve-fe` CI
gate resolves every `@concertable/*` intra-dependency from the published feed, never from workspace
source, so a consumer PR touching both the shared shape and a route file can never pass in the same PR —
this must land as two). **Remaining — PR2 (delivery-gated on that publish):** admin drops
`showSearch`/`showMailbox`/`onHeightChange`, `AppLayout` stops relying on `Navbar`'s internal
`showSearch`/`showMailbox` and instead owns `NavbarSearch` + the messaging slot itself via `endSlot`, and
`showSearch`/`showMailbox`/the internal `Mailbox` render finally come out of `Navbar` — then delete this
entry.
