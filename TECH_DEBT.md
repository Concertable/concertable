# Concertable — root technical debt

Debt that is genuinely repo-wide: `.github/workflows/**` gating logic, root-level docs/config, or
anything spanning both `api/` and `app/`. Backend-only cross-cutting debt (multiple services, host
`Program.cs` files) belongs in [`api/TECH_DEBT.md`](./api/TECH_DEBT.md); frontend-only cross-cutting
debt in [`app/web/TECH_DEBT.md`](./app/web/TECH_DEBT.md) / [`app/shared/TECH_DEBT.md`](./app/shared/TECH_DEBT.md).
Service- or tier-specific debt belongs in that area's own `TECH_DEBT.md`.

---

## MED

### The merge queue's `run_code` gate doesn't exclude docs/meta-only diffs

`.github/workflows/test.yml`'s `changes` job computes `run_fe` and `run_e2e` with path filters that
skip frontend/E2E work when nothing relevant changed, but the `run_code` gate — which controls the
entire backend `build`, every `carve-*` job, and the full `unit-tests`/`integration-tests` matrix
(~50 jobs) — does not have an equivalent exclusion for a diff that touches only docs/meta paths
(`**/*.md`, `.agents/**`, `.claude/**`, `plans/**`, `docs/**` — the same in-scope list `docs-review`
and `merge-docs` already use).

Confirmed live on PR #579 (docs-only: `**/*.md` + `.agents/hooks/docs_reachability.py`, no `api/**` or
`app/**` touched): `fe-boundaries` and `carve-fe` correctly showed `skipped`, but `build`, all five
`carve-*` jobs, and every `unit-tests`/`integration-tests` job ran and passed — burning the full
~10 minute merge-queue matrix on a change with zero backend blast radius. The `merge-docs` skill's
`--admin` bypass exists specifically to avoid this, but a docs-only PR routed through the ordinary
`--auto` queue path (no `--admin`) pays the full cost every time.

Durable fix: extend `run_code`'s path filter (mirroring `run_fe`'s pattern in the same job) to also
evaluate `false` when every changed path matches the docs/meta-only allowlist, so the backend matrix
is skipped the same way `run_fe`/`run_e2e` already are. This is a `.github/workflows/**` change — out
of scope for `merge-docs`, needs its own PR through `/merge`.

**Resolves when:** a diff touching only docs/meta paths evaluates `run_code=false` in the `changes`
job, and the merge queue's backend build/carve/test matrix is skipped for it the same way `run_fe` is
skipped today.
