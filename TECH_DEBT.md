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

The `this.` half of this entry is **not** closed yet. The intended convention is that `this.` exists
only to disambiguate a member a parameter or local shadows, and the codebase sweep has followed it —
but every published `dotnet-standards` copy still carries
`dotnet_style_qualification_for_field = true:error` in `STYLE.md`'s table plus the "every constructor
assignment is `this.`-qualified" prose, so the standard and the codebase disagree and every review of
this repo re-flags the sweep as a violation. The remaining sweep itself is tracked in
[`api/TECH_DEBT.md`](./api/TECH_DEBT.md).

**Also resolves when:** `STYLE.md` in `dotagents` states the disambiguation-only rule and that version
is published, so a reviewer reading the standard reaches the same conclusion as the codebase.

**Resolves when:** `csharp_prefer_braces = when_multiline:error` is enforced with the codebase brought
to it, and `STYLE.md`'s table matches the `.editorconfig`.
