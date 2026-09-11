# Rate limiting — Concertable's policy roster

The decision rule — throttle only on the trust axis or the cost axis, no global limiter, named opt-in
policies — is the `rate-limiting` skill. This file is the roster of the policies that exist here and the seam
they run through; it does not restate the rule.

## The seam

`Concertable.ServiceDefaults.RateLimitingExtensions` is the shared, web-only rate-limiting seam (kept out of
`AddServiceDefaults`, which non-web hosts also call). A web host opts in with `AddDefaultRateLimiting()` +
`UseDefaultRateLimiting()`, then declares each surface with
`AddRateLimitPolicy(name, new RateLimitWindow { PermitLimit, WindowSeconds }, perUser)`. Each window binds
from `RateLimiting:<policy>` in configuration, so an environment tightens it without a redeploy; rejection is
429. An endpoint opts in with `[EnableRateLimiting(RateLimitPolicies.X)]`.

## The policies

Names are `RateLimitPolicies` constants — `Concertable.B2B.Tenant.Contracts.RateLimitPolicies` for
B2B/Customer, `Concertable.Auth.RateLimitPolicies` for Auth. `perUser: false` partitions on client IP (the
trust axis); `perUser: true` partitions on the caller's `sub` (the cost axis).

| Policy | Axis · key | Guards |
|---|---|---|
| `PublicRead` | trust · per-IP | anonymous marketplace/profile browse (`ArtistController`, `VenueController`, reviews) |
| `Credential` (Auth) | trust · per-IP | login / token issue |
| `Upload`, `ProfileImage` | cost · per-user | image uploads (`BlobController`, profile images) |
| `Apply`, `Checkout` | cost · per-user | booking application + payment (`ApplicationController`) |
| `Messaging` | cost · per-user | sending messages (`MessageController`) |
| `ChangePassword` (Auth) | cost · per-user | password change |
| `Sensitive` | cost · per-user | expensive/destructive authenticated ops — GDPR subject erasure + export (`SubjectRightsController`) |

Adding one: add the constant to `RateLimitPolicies` (and its `All` list), register it in the host's
`AddRateLimitPolicy` block, and decorate the endpoint with `[EnableRateLimiting(...)]`. Only add a policy
when the endpoint trips an axis — the `rate-limiting` skill owns that call.
