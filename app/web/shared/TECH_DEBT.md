# app/web/shared technical debt

## LOW

### `stripeAccountApi.getPaymentMethod`'s return type says `undefined`, but the wire value is `null`

`StripeAccountController.GetPaymentMethod` returns `Ok(await payoutAccountService.GetPaymentMethodAsync(...))`
— a 200 with a JSON `null` body when there is no saved payment method, never a 204. `getPaymentMethod`
(`features/payments/api/stripeAccountApi.ts`) types the call as
`paymentClient.get<PaymentMethod | undefined>(...)` and returns `data` unchanged, so the real runtime value
is `null` while the declared type claims `undefined`. Not a live bug today — `usePaymentMethodQuery` passes
it straight as `queryFn`, and TanStack Query v5 only throws on an `undefined` resolution, not `null` — but
the type is a lie a future caller (an `=== undefined` check, a stricter lint rule) could trust and break on.
Same root shape as the `organizationApi.get` / `venueApi.getMyVenue` / `artistApi.getMyArtist` 204-vs-null
fixes; this one just isn't reachable yet because the backend already returns 200/`null`.

**Resolves when:** `getPaymentMethod` returns `Promise<PaymentMethod | null>`, matching the actual wire
value.

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
