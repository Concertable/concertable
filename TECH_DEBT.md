# Concertable — root technical debt

Debt that is genuinely repo-wide: `.github/workflows/**` gating logic, root-level docs/config, or
anything spanning both `api/` and `app/`. Backend-only cross-cutting debt (multiple services, host
`Program.cs` files) belongs in [`api/TECH_DEBT.md`](./api/TECH_DEBT.md); frontend-only cross-cutting
debt in [`app/web/TECH_DEBT.md`](./app/web/TECH_DEBT.md) / [`app/shared/TECH_DEBT.md`](./app/shared/TECH_DEBT.md).
Service- or tier-specific debt belongs in that area's own `TECH_DEBT.md`.

---

## MED

### One style rule the standard requires enforced is missing from `.editorconfig`

`STYLE.md` opens by stating that style rules an analyzer can express belong in `.editorconfig` at
`severity = error`, and lists five. The single root `.editorconfig` carries four — the private-field
camelCase naming rule, `csharp_style_namespace_declarations`, `MA0053` and `CA1848` — and omits
`csharp_prefer_braces = when_multiline:error`, so brace style on single-statement bodies is carried by
reviewers noticing.

The `this.` half of this entry is closed: `dotnet_style_qualification_for_field = true:error` has been
removed from `STYLE.md`'s table, because `this.` now exists only to disambiguate a member a parameter
or local shadows and that rule is all-or-nothing in `.editorconfig`. What remains of it is the
codebase sweep, tracked in [`api/TECH_DEBT.md`](./api/TECH_DEBT.md).

**Resolves when:** `csharp_prefer_braces = when_multiline:error` is enforced with the codebase brought
to it, and `STYLE.md`'s table matches the `.editorconfig`.
