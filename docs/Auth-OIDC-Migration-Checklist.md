# Auth migration: OAuth + OIDC (User & Role together)

Auth lives in a separate service. Concertable is an OIDC client. User and Role are one (in token / UserDto), so no separate Identity Roles or `FirstRole`—use `User.Role` from claims.

---

## Phase 1: Auth service (new project)

- [ ] Create ASP.NET Core Web API project (e.g. `Concertable.Auth`) in solution.
- [ ] Add OpenIddict + ASP.NET Core Identity (or your own user store with User+Role together).
- [ ] Model: **User and Role together** (e.g. `User.Role` or single `UserType`), no separate Role entity/table if you prefer.
- [ ] Auth service hosts:
  - [ ] Authorize endpoint (authorization code + PKCE).
  - [ ] Token endpoint (access + refresh + ID token).
  - [ ] Login (and optionally consent) UI.
  - [ ] Discovery (e.g. `/.well-known/openid-configuration`) and JWKS.
- [ ] Register client for Concertable: client_id, redirect_uris, scopes (`openid`, `profile`), PKCE.
- [ ] Put role (or user type) in token: access token and/or ID token claim (e.g. `role` or `user_type`).

---

## Phase 2: Concertable as OIDC client

- [ ] Add **OpenID Connect** for web (browser): `AddOpenIdConnect()` with authority = Auth service URL, client id, client secret (or PKCE).
- [ ] Add **JWT Bearer** for API: `AddJwtBearer()` with same authority, validate issuer and audience.
- [ ] Remove (or phase out) cookie-based Identity sign-in for the main app (no more `SignInManager`/login page in Concertable).
- [ ] **CurrentUser from claims only**: middleware (or equivalent) builds `UserDto` from `HttpContext.User` claims (sub, email, role, etc.). No call to Auth service per request unless you want extra data.
- [ ] Remove dependency on `UserManager`/`SignInManager` in Concertable for “who is logged in” (optional: keep for any local user lookup until fully migrated).

---

## Phase 3: Remove FirstRole — User and Role together

- [ ] **ICurrentUser**: Remove `GetFirstRole()`. Role is on the user: use `Get().Role` (or `User.Role` if you use properties).
- [ ] Replace every `currentUser.GetFirstRole()` with `currentUser.Get().Role` (or `currentUser.User.Role`). Ensure token/claims always include role so it’s never null when authenticated.
- [ ] Any “user has no roles” handling: either enforce in Auth service (always issue role) or a single check on `User.Role` where needed.

---

## Phase 4: Optional Refit (server-to-server to Auth)

- [ ] Only if Concertable backend needs to call Auth APIs (e.g. full profile, revoke, sync).
- [ ] Define Refit interface (e.g. `IAuthServiceApi`) for those endpoints.
- [ ] Register Refit client with Auth service base URL; send access token (e.g. from current request) in `Authorization` header.
- [ ] Use for “get user profile” or similar; keep normal request auth as “validate JWT and read claims” so you don’t need Refit on every request.

---

## Summary

| Before | After |
|--------|--------|
| Cookie auth in Concertable, Identity User + Role | Auth service issues tokens; User + Role in token/UserDto |
| `GetFirstRole()` | `Get().Role` (role with user, no separate API) |
| CurrentUser from UserManager in middleware | CurrentUser from JWT/claims in middleware |
| Optional | Refit client only if you add Auth HTTP API and need to call it from Concertable |
