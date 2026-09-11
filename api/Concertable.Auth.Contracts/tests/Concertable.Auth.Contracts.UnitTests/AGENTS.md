# Concertable.Auth.Contracts.UnitTests — unit tests

Covers the published `Concertable.Auth.Contracts` typed identity model — the `InteractiveClients`,
`AuthScopes` and `AuthResources` frozen catalogs: completeness, wire-id round-trips, `TryGet` misses, and
the `AuthParty` classification the cross-service registration handlers depend on. The project under test is
the sibling `api/Concertable.Auth.Contracts/` package (a Tests project, so the cross-folder reference is
carve-exempt).

**Unit-only: a test that needs a host, HTTP, a container or a database belongs elsewhere.**

Conventions: the `dotnet-standards:unit-testing` skill, plus `dotnet:unit-testing` for this system's
test-tier gate.
