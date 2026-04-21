# Integration Test API Client Refactor

## Problem

Integration test failures produce useless output:

```
Assert.Equal() Failure: Values differ
Expected: OK
Actual:   InternalServerError
```

The server's `GlobalExceptionHandler` already writes a rich `ProblemDetails` body (with stack trace, inner exception, exception type) — but the test reads `response.StatusCode` via `Assert.Equal` and never reads `response.Content`, so the body is discarded before reaching the test runner.

## Decision

Replace the static `HttpClient` extensions (`ReadAsync<T>`, `PostAsync<T>`, etc.) with a **typed HTTP client** registered via `IHttpClientFactory`. The client owns the full request→assert→deserialise flow in a single method, and body-on-failure is built into the DelegatingHandler pipeline so every test gets it automatically.

### Why typed client over static helper

- Static `AssertStatusAsync` works but requires touching every call site *and* leaves the door open for new tests to use raw `Assert.Equal(status, response.StatusCode)` and lose the body again.
- Typed client centralises the pattern: every call goes through `api.GetAsync<T>(url, expect)` and body-on-failure is guaranteed.
- `IHttpClientFactory` gives us a pipeline — new concerns (logging, retry, correlation IDs) slot in as `DelegatingHandler`s without touching tests.

### Why env-gated only on the server

The rich body comes from `GlobalExceptionHandler`, which is already gated on `IsDevelopment() || IsEnvironment("Testing")`. The test helper is unconditional — it just prints whatever the server returned. One env check, one place.

## Architecture

```
HttpClient (registered via AddHttpClient<ITestApiClient, TestApiClient>)
  │
  ├─ Primary handler:    WebApplicationFactory.Server.CreateHandler()
  │
  └─ DelegatingHandlers (ordered, outermost first):
       ├─ ResponseBodyCapturingHandler   ← buffers body so assertion can read it
       └─ AuthDelegatingHandler          ← reads TestUserContext from request.Options,
                                           writes X-Test-Sub / X-Test-Role headers
```

## Components to build

### 1. `TestUserContext` + `HttpRequestOptionsKey`

Carries the acting user per-request:

```csharp
public sealed record TestUserContext(Guid UserId, Role Role);

public static class TestRequestOptions
{
    public static readonly HttpRequestOptionsKey<TestUserContext> UserKey = new("test.user");
}
```

### 2. `ResponseBodyCapturingHandler : DelegatingHandler`

On every response, pre-read `response.Content` into a buffered `ByteArrayContent` so later reads don't hit a closed stream. Preserves original headers.

### 3. `AuthDelegatingHandler : DelegatingHandler`

Reads `TestUserContext` from `request.Options` (if present) and writes `TestAuthHandler.UserIdHeader` / `TestAuthHandler.RoleHeader`. If absent, request is anonymous.

### 4. `ITestApiClient` / `TestApiClient`

Public surface:

```csharp
public interface ITestApiClient
{
    ITestApiClient As(UserEntity user);
    ITestApiClient As(Guid userId, Role role);

    Task<T> GetAsync<T>(string url, HttpStatusCode expect = HttpStatusCode.OK);
    Task<HttpResponseMessage> GetAsync(string url, HttpStatusCode expect);

    Task<T> PostAsync<T, TBody>(string url, TBody body, HttpStatusCode expect = HttpStatusCode.OK);
    Task<HttpResponseMessage> PostAsync<TBody>(string url, TBody body, HttpStatusCode expect);
    Task<HttpResponseMessage> PostAsync(string url, HttpStatusCode expect);

    Task<T> PutAsync<T, TBody>(string url, TBody body, HttpStatusCode expect = HttpStatusCode.OK);
    Task<HttpResponseMessage> PutAsync<TBody>(string url, TBody body, HttpStatusCode expect);

    Task<HttpResponseMessage> DeleteAsync(string url, HttpStatusCode expect);
}
```

Behaviour:
- `As(user)` returns a **new** `TestApiClient` carrying a `TestUserContext` (immutable; keeps tests thread-safe across fixture reuse).
- Each method builds an `HttpRequestMessage`, stashes the `TestUserContext` (if any) into `request.Options[TestRequestOptions.UserKey]`, sends it, and checks the status.
- On status mismatch: read body, pretty-print JSON if parseable, throw `XunitException` with method + URI + status + body.
- On match: deserialise from JSON and return.

### 5. `ApiFixture` changes

Replace the `CreateClient(...)` overloads with:

```csharp
public ITestApiClient Api { get; private set; }   // anonymous
public ITestApiClient As(UserEntity user) => Api.As(user);
public ITestApiClient As(Guid userId, Role role) => Api.As(userId, role);
```

Registration inside `ConfigureTestServices`:

```csharp
services.AddTransient<AuthDelegatingHandler>();
services.AddTransient<ResponseBodyCapturingHandler>();

services.AddHttpClient<ITestApiClient, TestApiClient>()
    .ConfigurePrimaryHttpMessageHandler(() => factory.Server.CreateHandler())
    .AddHttpMessageHandler<ResponseBodyCapturingHandler>()
    .AddHttpMessageHandler<AuthDelegatingHandler>();
```

The existing `TestClientOptions`-based `CreateClient` overloads (failing Stripe, failing geocoding, etc.) remain as escape hatches — they spin up their own `WebApplicationFactory` with service overrides and return a plain `HttpClient`. These don't fit the "one typed client registered at startup" model cleanly; leave them alone for now and migrate later if wanted.

### 6. Delete

- `WebApplicationHttpClientFactory` — obsolete.
- `HttpClientExtensions.AssertStatusAsync` — superseded by typed client.
- Eventually: the rest of `HttpClientExtensions` once all call sites are migrated.

## Test migration

Before:
```csharp
var client = fixture.CreateClient();
var response = await client.GetAsync("/api/Header?headerType=Artist&searchTerm=Test");
Assert.Equal(HttpStatusCode.OK, response.StatusCode);
var result = await response.Content.ReadAsync<PaginationResponse<ArtistHeaderDto>>();
Assert.NotNull(result);
Assert.Single(result.Data);
```

After:
```csharp
var result = await fixture.Api.GetAsync<PaginationResponse<ArtistHeaderDto>>(
    "/api/Header?headerType=Artist&searchTerm=Test");
Assert.Single(result.Data);
```

Authed version:
```csharp
var result = await fixture.As(user).GetAsync<OpportunityDto>($"/api/opportunity/{id}");
```

Error-path version (what motivated this whole thing):
```csharp
await fixture.As(otherManager).PostAsync(
    $"/api/opportunityApplication/{id}/accept",
    HttpStatusCode.BadRequest);
// failure now dumps full ProblemDetails with stack trace
```

## Sweep plan

1. Build infrastructure (components 1–5 above).
2. Migrate `HeaderApiTests.cs` end-to-end as proof.
3. Run that test file. Confirm body-on-failure works by inducing a 500.
4. Green-light for full sweep — migrate remaining test files:
   - `Controllers/Artist/ArtistApiTests.cs`
   - `Controllers/Auth/AuthApiTests.cs`
   - `Controllers/Concert/ConcertApiTests.cs`
   - `Controllers/Opportunity/OpportunityApiTests.cs`
   - `Controllers/OpportunityApplication/*` (5 files)
   - `Controllers/Search/*` (3 remaining)
   - `Controllers/Ticket/*` (5 files)
   - `Controllers/User/UserApiTests.cs`
   - `Controllers/Venue/VenueApiTests.cs`
5. Delete `HttpClientExtensions` and `WebApplicationHttpClientFactory`.
6. Confirm full test run is green.

## Out of scope for this refactor

- `TestClientOptions`-driven per-test service overrides (failing Stripe, failing geocoding). Keep the existing `CreateClient(user, configure)` overloads.
- Non-integration test projects (`Concertable.Core.IntegrationTests`, `Concertable.Infrastructure.IntegrationTests`) unless they also use `HttpClientExtensions`.
- Production HTTP client patterns — unrelated.

## Risk / rollback

- The `DelegatingHandler` pipeline sits between every test and the server. If buffering breaks large responses or streaming endpoints, we'll see it immediately.
- Auth moves from `client.DefaultRequestHeaders` to a per-request mechanism. Any test that relied on header persistence across calls on the same client will need explicit `.As(user)` on each call — but `TestApiClient` makes this the default.
- Rollback: delete the new files, restore `HttpClientExtensions` + `CreateClient` overloads, revert test sweep. The server-side `GlobalExceptionHandler` env-gate change stands either way.
