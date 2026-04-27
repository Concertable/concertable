# Concertable

## Migrations

Don't add additive migrations. When the model changes, run `./initial-migrations.ps1` from `api/`
to nuke and re-scaffold every module's `InitialCreate`.

## Module rules

See [MODULAR_MONOLITH_RULES.md](./MODULAR_MONOLITH_RULES.md).
