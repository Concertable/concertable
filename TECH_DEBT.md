# Concertable — root technical debt

Debt that is genuinely repo-wide: `.github/workflows/**` gating logic, root-level docs/config, or
anything spanning both `api/` and `app/`. Backend-only cross-cutting debt (multiple services, host
`Program.cs` files) belongs in [`api/TECH_DEBT.md`](./api/TECH_DEBT.md); frontend-only cross-cutting
debt in [`app/web/TECH_DEBT.md`](./app/web/TECH_DEBT.md) / [`app/shared/TECH_DEBT.md`](./app/shared/TECH_DEBT.md).
Service- or tier-specific debt belongs in that area's own `TECH_DEBT.md`.

---

## MED

### Two style rules the standard requires enforced are advisory in `.editorconfig`

`STYLE.md` opens by stating that style rules an analyzer can express belong in `.editorconfig` at
`severity = error`, and lists six. The single root `.editorconfig` carries four — the private-field
camelCase naming rule, `csharp_style_namespace_declarations`, `MA0053` and `CA1848` — and omits two:

| Missing setting | Effect of the omission |
|---|---|
| `csharp_prefer_braces = when_multiline:error` | Brace style on single-statement bodies is carried by reviewers noticing |
| `dotnet_style_qualification_for_field = true:error` | `this.` on field access is carried the same way |

Neither is a one-line add. `dotnet_style_qualification_for_field = true` requires `this.` on *every*
field access, and the backend does the opposite: production services qualify only in constructors, for
parameter disambiguation, and read fields bare in method bodies — `BookingWorkflow`,
`DealStrategyBuilder`, `TenantContext` and `AsbTopology` are all shaped that way, while the
non-constructor qualification that does exist is concentrated in test classes (`this.sut`,
`this.repository.Setup`). Enabling it at `error` would flag most method bodies in `api/src`, so the real
decision is which convention the standard should state, not which line to paste.

**Resolves when:** each of the two is either enforced at `error` with the codebase brought to it, or the
standard is amended to state the convention the codebase actually holds — and `STYLE.md`'s table matches
the `.editorconfig` either way.
