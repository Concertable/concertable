# Code review — Fix/HostingContainerEndpoints

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `9f4b26ebbd61fe5c5ca517d053098bea2aa7e1db`  _(2026-09-10)_

> Range reviewed: `ea985630f..9f4b26ebb` (2 commits, 9 files, +149).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **Low — the new tests model Auth with the endpoint shape this change exists to correct.**
  All three `ImageCompositionTests` build their Auth stand-in with
  `.WithHttpsEndpoint(targetPort: AuthConstants.ContainerPort, name: "https")`
  (`api/Concertable.B2B/tests/Concertable.B2B.StartupTests/ImageCompositionTests.cs:27`,
  `api/Concertable.Customer/tests/Concertable.Customer.StartupTests/ImageCompositionTests.cs:26`,
  `api/Concertable.Search/tests/Concertable.Search.StartupTests/ImageCompositionTests.cs:24`),
  while each asserts `Assert.Equal("http", endpoint.UriScheme)` for the service under test. The Auth
  image binds plaintext on 8080 exactly as the three web images do — it sets `ASPNETCORE_HTTP_PORTS=8080`,
  declares no HTTPS port and ships no certificate — so `WithHttpsEndpoint` declares a scheme the artefact
  does not serve, and a consumer building `https://` against that endpoint negotiates TLS with a cleartext
  port. Nothing asserts on the stand-in and the app is never started, so there is no behavioural effect
  here; the cost is that the file teaching the correct shape demonstrates the incorrect one three times,
  and the setup is what a reader copies. Change all three to
  `.WithHttpEndpoint(targetPort: AuthConstants.ContainerPort, name: "https")`.
  Fixed in all three; the Search suite rebuilds and its test passes.

The native correctness, reuse, simplification, efficiency and error-handling pass is otherwise clean.
`WithHttpEndpoint(..., name: "https")` on each image overload matches the shape `AddPaymentWeb` already
established, the three `ContainerPort` constants agree with what the published images actually expose,
and the constants sit with the existing resource-name vocabulary in each service's constants type. The
routed `csharp-style` and `csharp-naming` rules are satisfied. No security-sensitive path changed, so no
security pass was required and no security marker is stamped.

## Out of scope, raised for the plan rather than this branch

- `Concertable.Auth.Hosting`'s image overload still has its endpoint declared by the caller
  (`Concertable/system`'s AppHost) rather than by the package, so Auth remains the one service whose
  composition seam is incomplete after this lands.
- `Concertable/system` pins eight services, three of which are workers with no HTTP surface. Its
  `EveryPinnedService_ExposesAnEndpoint` expectation cannot hold for those three, and this change does
  not address it.
