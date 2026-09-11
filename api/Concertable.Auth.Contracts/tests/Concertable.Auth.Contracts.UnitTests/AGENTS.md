# Concertable.Auth.Contracts.UnitTests — unit tests

Covers the published `Concertable.Auth.Contracts` typed identity model — `InteractiveClientInfo`,
`AuthScopes` and `AuthResources`, each a frozen catalog owning its own lookup: completeness, wire-id
round-trips, `GetOrDefault` misses. The project under test is the sibling `api/Concertable.Auth.Contracts/`
package (a Tests project, so the cross-folder reference is carve-exempt).

**Unit-only: a test that needs a host, HTTP, a container or a database belongs elsewhere.**

Conventions: the `dotnet-standards:unit-testing` skill, plus `dotnet:unit-testing` for this system's
test-tier gate.
