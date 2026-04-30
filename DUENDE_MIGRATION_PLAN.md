# Duende Identity Server Migration Plan

## What We're Doing and Why

Right now Concertable rolls its own auth: `JwtTokenService` manually signs JWTs with an HMAC key,
`BCryptPasswordHasher` hashes passwords, and `RefreshTokenRepository` manages refresh token rotation
in the database. It works, but every piece is custom and therefore every piece is your responsibility
to get right — key rotation, token replay attacks, clock skew, refresh token theft. That's the
"don't do it yourself" problem.

Duende Identity Server replaces all of that with a standards-compliant OAuth2/OIDC authorization
server. `Concertable.Web` stops issuing tokens entirely. `Concertable.Auth` (a new dedicated host)
becomes the single source of truth for authentication. Every token `Concertable.Web` accepts was
signed by the IS — it just verifies the signature, it doesn't create anything.

---

## Concepts You Need for Interviews

### OAuth2 vs OIDC

**OAuth2** is an authorisation protocol. It defines how a client (your SPA) gets permission to access
a resource (your API) on behalf of a user. It's about *what you can do*.

**OpenID Connect (OIDC)** is an identity layer on top of OAuth2. It adds the concept of *who you are*
by introducing the ID token — a JWT that carries the user's identity claims (sub, email, name etc.).
OAuth2 gives you an access token. OIDC gives you an access token *and* an ID token.

Interview framing: "OAuth2 handles authorisation, OIDC handles authentication. OIDC is OAuth2 with
an identity contract bolted on."

---

### The Three Roles

| Role | Who | Responsibility |
|---|---|---|
| **Authorization Server (AS)** | `Concertable.Auth` (Duende IS) | Issues and signs tokens after verifying the user |
| **Resource Server (RS)** | `Concertable.Web` | Accepts requests, validates tokens, serves data |
| **Client** | Your SPA (Angular/Vue) | Requests tokens from the AS and sends them to the RS |

The user never talks to the AS directly (except on the login page). After login, the AS gives the SPA
a token, and the SPA uses that token on every request to the API. The API trusts the token because it
was signed by the AS — not because it issued it.

---

### Grant Types

A grant type is the method by which a client gets a token from the AS.

**Authorization Code + PKCE** — the correct choice for a SPA.
1. User clicks "Login" on the SPA.
2. SPA redirects the browser to `Concertable.Auth/connect/authorize`.
3. User logs in on the IS login page (Razor Pages you write).
4. IS redirects back to the SPA with a short-lived **authorization code**.
5. SPA exchanges the code for tokens at `/connect/token`.
6. PKCE (Proof Key for Code Exchange) is a cryptographic challenge sent in step 2 and verified in
   step 5 — it prevents an intercepted code from being used by a malicious actor.

**Client Credentials** — machine-to-machine (no user). Workers calling other services. No login page.

**Resource Owner Password Credentials (ROPC)** — client sends username/password directly to the token
endpoint. No redirect. Duende supports it but it's legacy and disabled by default. We won't use it.

---

### Token Types

**Access Token** — short-lived JWT (15 min). The SPA sends this as `Authorization: Bearer <token>` on
every API request. The API validates its signature and expiry — it does NOT call the IS on every
request (that would be slow). The signature is enough.

**ID Token** — JWT containing the user's identity (sub, email, role). Used by the *client* (SPA) to
know who is logged in. Never sent to the API.

**Refresh Token** — long-lived opaque token. The SPA uses this to get a new access token when the
current one expires. Sent only to the IS, never to the API.

---

### Discovery Endpoint

Every OIDC-compliant server exposes:
```
GET /.well-known/openid-configuration
```
This returns a JSON document listing the server's capabilities, token endpoint URL, signing keys
(JWKS URI), supported grant types etc.

`Concertable.Web` uses this to automatically download the public signing key and validate incoming
tokens. You point it at the IS URL and it figures everything out. No manual key distribution.

---

### Scopes and Claims

**Scope** — a label representing a set of permissions. Examples: `openid`, `profile`, `concertable.api`.
The client requests scopes. The IS grants them (or not). The access token contains the granted scopes.

**Claim** — a key-value pair inside a token. `sub` = user ID, `email` = email address, `role` = role.
`IProfileService` is where you tell Duende IS *which claims to put into tokens for a given user*.

---

## Current State

```
Concertable.Auth module (inside Concertable.Web process)
  ├── JwtTokenService        — manually creates JWTs
  ├── BCryptPasswordHasher   — hashes passwords
  ├── RefreshTokenRepository — manages refresh tokens in AuthDbContext
  ├── AuthService            — orchestrates register/login/logout/refresh/email-verify/reset-password
  └── AuthController         — HTTP surface for all of the above

Concertable.Web
  └── AddAuthentication().AddJwtBearer(...)  — validates tokens using local symmetric key
```

---

## Target State

```
api/
  Concertable.Auth/         ← NEW dedicated host (own Program.cs, own port in Aspire)
    Config.cs               — defines clients, scopes, resources
    Pages/Account/          — login, logout, error Razor Pages
    Services/
      ProfileService.cs     — IProfileService: maps UserEntity → claims
      UserValidator.cs      — validates username/password against User module DB

  Concertable.Web/
    Program.cs              — AddAuthentication().AddJwtBearer() pointing at IS discovery endpoint
    (no more Auth module reference)

  Modules/Auth/             ← DELETED
```

---

## Stage 1 — Understand What You're Building

Before writing any code, make sure you can answer these:

- What is the difference between an access token and an ID token?
- What problem does PKCE solve?
- Why does the API not need to call the IS on every request?
- What is `IProfileService` and why does Duende IS need it?

These come up in interviews. The answers are in the Concepts section above.

---

## Stage 2 — Create `Concertable.Auth`

**What:** A new ASP.NET Core Web project at `api/Concertable.Auth/`.

**Why it's a separate host:** The IS has its own discovery endpoint, its own login UI, its own signing
key management. It's a separate *concern* from the API. In production, you could deploy it to a
different server. Aspire makes multi-host local development trivial.

**Steps:**
1. `dotnet new web -n Concertable.Auth -o api/Concertable.Auth`
2. Add NuGet packages:
   - `Duende.IdentityServer` — the core package
   - `Duende.IdentityServer.EntityFramework` — for EF-backed stores (Stage 8)
3. Add to the solution: `dotnet sln add api/Concertable.Auth/Concertable.Auth.csproj`
4. Add `Concertable.ServiceDefaults` reference (Aspire health checks, telemetry)

**Interview question you'll get:** "Why is the identity server a separate project/service?"
Answer: Separation of concerns. The auth server's job is to authenticate users and issue tokens.
The API's job is to serve business data. If you embed auth in the API, a bug in one affects the
other. As a separate host, you can scale, deploy, and update them independently.

---

## Stage 3 — Configure Identity Server (`Config.cs`)

**What:** Define the static configuration — what resources exist, what clients can access them.

**Key objects:**

```csharp
// What API scopes exist (what the access token can grant access to)
public static IEnumerable<ApiScope> ApiScopes =>
[
    new ApiScope("concertable.api", "Concertable API")
];

// What identity resources exist (what goes in the ID token)
public static IEnumerable<IdentityResource> IdentityResources =>
[
    new IdentityResources.OpenId(),   // sub claim — required for OIDC
    new IdentityResources.Profile(),  // name, email etc.
    new IdentityResource("roles", "User roles", ["role"])
];

// Who can request tokens (your SPA)
public static IEnumerable<Client> Clients =>
[
    new Client
    {
        ClientId = "concertable-spa",
        AllowedGrantTypes = GrantTypes.Code,   // Authorization Code
        RequirePkce = true,                    // + PKCE
        RequireClientSecret = false,           // SPAs can't keep secrets
        RedirectUris = ["http://localhost:4200/callback"],
        PostLogoutRedirectUris = ["http://localhost:4200"],
        AllowedScopes = ["openid", "profile", "roles", "concertable.api"],
        AllowOfflineAccess = true              // enables refresh tokens
    }
];
```

**Why `RequireClientSecret = false` for a SPA:** A SPA runs in the browser. Any "secret" you put in
JavaScript is visible to anyone who opens DevTools. PKCE replaces the client secret for public clients.

**Interview question:** "What is a client in OAuth2?"
Answer: Any application that requests tokens from the authorisation server. Each client has a
`ClientId` and defines which grant types and scopes it's allowed to use. The IS won't issue tokens
to an unregistered client.

---

## Stage 4 — Implement `IProfileService`

**What:** The bridge between Duende IS and your user store. When IS needs to put claims into a token,
it calls `IProfileService.GetProfileDataAsync()`. You load the user from the database and return the
claims.

**Why this matters:** Duende IS doesn't know about your `UserEntity`. It only knows about standards-
compliant claims. `IProfileService` is where you translate your domain model into JWT claims.

```csharp
public class ConcertableProfileService : IProfileService
{
    private readonly IUserModule users;

    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var userId = Guid.Parse(context.Subject.GetSubjectId());
        var user = await users.GetByIdAsync(userId);

        context.IssuedClaims.AddRange([
            new Claim("email", user.Email),
            new Claim("role", user.Role.ToString())
        ]);
    }

    public async Task IsActiveAsync(IsActiveContext context)
    {
        var userId = Guid.Parse(context.Subject.GetSubjectId());
        var user = await users.GetByIdAsync(userId);
        context.IsActive = user is not null;
    }
}
```

**`IsActiveAsync`** — called before issuing a refresh token to check if the user is still active
(not deleted/banned). This is important for security.

**Interview question:** "How does Duende IS know what to put in the access token?"
Answer: Via `IProfileService`. IS calls `GetProfileDataAsync` with the user's `sub` (subject ID) and
a context indicating which scopes were requested. You load the user and add the relevant claims. IS
handles the token structure, signing, and expiry — you just provide the payload.

---

## Stage 5 — Build the Login/Logout UI

**What:** Razor Pages that Duende IS redirects to during the auth flow. These are *your pages* —
Duende IS just calls them via redirect.

**Why Razor Pages (not API endpoints):** The Authorization Code flow works via browser redirects.
The user's browser hits the IS authorize endpoint, IS redirects to your login page, user submits
the form, your page calls `HttpContext.SignInAsync()`, IS continues the flow and redirects back to
the SPA. This can't be done with a REST API — it requires a rendered HTML form.

**Pages to create:**
- `Pages/Account/Login.cshtml` — username/password form
- `Pages/Account/Logout.cshtml` — confirmation page
- `Pages/Error.cshtml` — IS error display

Duende's [Quickstart UI](https://github.com/DuendeSoftware/IdentityServer.Quickstart.UI) is a set of
pre-built Razor Pages you can copy in directly. You don't write these from scratch.

**What your login page does (simplified):**
1. User submits email + password
2. Call `IUserModule.GetCredentialsByEmailAsync(email)` and verify password with BCrypt
3. On success, call `await HttpContext.SignInAsync(userId, ...)` — this signals to Duende IS that the
   user is authenticated
4. Duende IS generates the authorization code and redirects back to the SPA's `RedirectUri`

**Interview question:** "Where does the login UI live in a Duende IS setup?"
Answer: In the IS host itself, as Razor Pages. The AS is the only party that handles credentials —
the client application (SPA or native app) never sees the username/password. This is the security
benefit of the redirect-based flow.

---

## Stage 6 — Wire Into Aspire AppHost

**What:** Register `Concertable.Auth` as a project in the AppHost so it gets its own URL, health
checks, and telemetry in the Aspire dashboard.

```csharp
var auth = builder.AddProject<Projects.Concertable_Auth>("concertable-auth")
    .WithReference(sql);

var api = builder.AddApi(sql)
    .WithReference(auth);  // web needs to know the IS URL for token validation
```

The IS URL (`https://localhost:5001`) gets injected into `Concertable.Web`'s configuration so the
JWT bearer middleware knows where to fetch the discovery document.

---

## Stage 7 — Update `Concertable.Web` to a Pure Resource Server

**What:** `Concertable.Web` stops doing anything auth-related except validating incoming tokens.

**Current (`AddJwtBearer` with local key):**
```csharp
services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(settings.JwtSigningKeyBase64)),
            ValidIssuer = settings.Issuer,
            ValidAudience = settings.Audience
        };
    });
```

**New (`AddJwtBearer` pointing at IS):**
```csharp
services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.Authority = configuration["Auth:Authority"]; // "https://localhost:5001"
        options.Audience = "concertable.api";
        // IS public keys are fetched automatically from /.well-known/openid-configuration
    });
```

That's it. `Concertable.Web` no longer knows about signing keys. It trusts the IS to sign correctly,
fetches the public key from the discovery endpoint on startup, and validates every incoming token
against it. Key rotation (changing the signing key) becomes a IS concern, not an API concern.

**What stays the same:** `ICurrentUser`, `[Authorize]` attributes, role-based policies — all of this
continues to work because the claims (`sub`, `role`, `email`) are still in the access token.

---

## Stage 8 — Delete the Old Auth Module

Once Stage 7 is verified working, remove:

- `api/Modules/Auth/` — all 7 projects
- `services.AddAuthApi(...)` and `services.AddAuth(...)` from `Concertable.Web/Program.cs`
- `services.AddAuthDevSeeder()` / `services.AddAuthTestSeeder()`
- `Microsoft.AspNetCore.Authentication.JwtBearer` package from `Concertable.Web.csproj`
  (replaced by the IS-aware one)
- Auth module's `AuthDbContext` and its migrations
- Run `./initial-migrations.ps1` from `api/` to re-scaffold (Auth schema tables are gone)

**What you're keeping from the old Auth module's concepts:**
- BCrypt password hashing — moves into `Concertable.Auth`'s login page handler
- Email verification — moves to Stage 9
- Password reset — moves to Stage 9

---

## Stage 9 — Email Verification and Password Reset

**What:** These flows don't exist in the OAuth2/OIDC spec. Duende IS handles token issuance, not
account lifecycle. You have two options:

**Option A:** Custom Razor Pages in `Concertable.Auth`
- Keeps auth-related UI in one place
- `Concertable.Auth` calls `IUserModule` (via project reference to User.Infrastructure) to
  send emails and update the verified flag

**Option B:** API endpoints in the User module
- Simpler — stays in the existing module structure
- Email verification link hits `POST /api/user/verify-email?token=xxx`
- Password reset link hits `POST /api/user/reset-password`

Option B is less friction. The flows are account management, not authentication — they don't need
to be in the IS host. The distinction: the IS issues tokens to *already-authenticated* users.
Email verification happens *before* a user is allowed to authenticate.

---

## Stage 10 — EF-Backed Operational Store (Production-Grade, Optional but Impressive)

**What:** By default, Duende IS stores active tokens, grants, and keys in memory. That means they're
lost on restart. The EF-backed operational store persists them to SQL Server.

```csharp
builder.Services.AddIdentityServer()
    .AddOperationalStore(options =>
    {
        options.ConfigureDbContext = b => b.UseSqlServer(connectionString);
        options.EnableTokenCleanup = true;  // background job removes expired tokens
    });
```

**Why this matters for interviews:** It shows you understand that the IS is stateful (refresh tokens,
consent grants, device codes all need to survive restarts in production). This is the production
configuration. In-memory is development-only.

**Interview question:** "What happens to refresh tokens if the IS restarts with in-memory storage?"
Answer: They're lost. Every logged-in user is silently logged out on the next API call. The
operational store (EF, Redis, etc.) persists these across restarts — essential for production.

---

## Stage 11 — Update Integration Tests

**What:** Tests currently rely on the Auth module to issue tokens. With the IS as a separate host,
the test setup needs to change.

**Options:**

**Option A — Mock the token (simpler):** In integration tests, bypass the IS entirely. Create test
JWTs directly with the IS's signing key (available in test config). The `WebApplicationFactory`
configures `AddJwtBearer` to accept locally-signed test tokens.

**Option B — Spin up the IS in tests:** Use `WebApplicationFactory<Concertable.Auth.Program>` 
alongside the Web factory. Tests get real tokens via the ROPC grant (acceptable in test environments
even if not production).

Option A is standard for modular monolith integration tests — you don't want test reliability to
depend on a second process. The pattern is: configure a known signing key in test appsettings,
create JWTs in the test seeder with that key, `Concertable.Web` validates against the same key.

---

## Summary Checklist

| Stage | What | Done |
|---|---|---|
| 1 | Create `Concertable.Auth` project | ☐ |
| 2 | Configure IS: clients, scopes, resources | ☐ |
| 3 | Implement `IProfileService` | ☐ |
| 4 | Build login/logout Razor Pages | ☐ |
| 5 | Wire into Aspire AppHost | ☐ |
| 6 | Update `Concertable.Web` to resource server | ☐ |
| 7 | Delete old Auth module | ☐ |
| 8 | Handle email verify + password reset | ☐ |
| 9 | EF operational store | ☐ |
| 10 | Update integration tests | ☐ |

---

## Key Interview Questions This Demonstrates You Can Answer

1. What is the difference between OAuth2 and OIDC?
2. What are the roles in an OAuth2 system?
3. Why do SPAs use Authorization Code + PKCE instead of implicit flow?
4. What is a refresh token and why is it only sent to the auth server?
5. How does an API validate a JWT without calling the auth server on every request?
6. What is `IProfileService` and when does Duende IS call it?
7. What is the discovery endpoint and why does it matter?
8. Why would you use a separate authorization server rather than rolling your own JWTs?
9. What is token rotation and why does it matter for security?
10. How would you handle key rotation in a production IS setup?
