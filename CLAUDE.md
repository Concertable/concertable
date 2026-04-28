# Concertable

## Migrations

Don't add additive migrations. When the model changes, run `./initial-migrations.ps1` from `api/`
to nuke and re-scaffold every module's `InitialCreate`.

## DTOs vs Responses

Services return `Dto` types from `Module.Application/DTOs/` (or `Module.Contracts/` for cross-module
shapes). Services never return HTTP-flavoured `Response` types — keeps services callable from
non-HTTP consumers (Workers, gRPC, SignalR, etc.).

Controllers return either the Dto verbatim (default — most endpoints) or a `Response` from
`Module.Api/Responses/` if the wire shape genuinely differs from the Dto (versioning, role-based
shaping, HATEOAS, multiple endpoints rendering the same Dto differently). Don't pre-emptively
shadow every Dto with a Response.

Validators stay named `XValidators` regardless.

## Module rules

See [MODULAR_MONOLITH_RULES.md](./MODULAR_MONOLITH_RULES.md).
