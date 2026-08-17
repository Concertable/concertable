# Concertable — root technical debt

Debt that is genuinely repo-wide: `.github/workflows/**` gating logic, root-level docs/config, or
anything spanning both `api/` and `app/`. Backend-only cross-cutting debt (multiple services, host
`Program.cs` files) belongs in [`api/TECH_DEBT.md`](./api/TECH_DEBT.md); frontend-only cross-cutting
debt in [`app/web/TECH_DEBT.md`](./app/web/TECH_DEBT.md) / [`app/shared/TECH_DEBT.md`](./app/shared/TECH_DEBT.md).
Service- or tier-specific debt belongs in that area's own `TECH_DEBT.md`.

---

## MED

### `platform-sync.yml`'s "Enable auto-merge" step has no retry — a transient 503 strands the sync PR forever

`chore/platform-sync-0.1.0-alpha.0.1049` (#634) sat green with `autoMergeRequest: null` — its own PR
body says "Auto-merge is on," but nothing had actually armed it. The workflow run that opened it
(`32024445332`) shows why: the "Enable auto-merge" step's `gh pr merge --auto "$branch"` call failed
with `non-200 OK status code: 503 Service Unavailable body: "upstream connect error or disconnect/reset
before headers. reset reason: connection termination"` — a one-off transient GitHub API blip, not a
logic bug (the step's own command is already correct — no `--squash`/`--merge` flag, matching the
in-file comment documenting that exact prior footgun). Nothing retries this step, and nothing else in
the workflow (or `platform-sync-alert.yml`, which only watches for a *red* sync PR) notices a *green*
sync PR that never got enqueued — so it just sits there until a human happens to look, exactly the
"case 4: green but never admitted" failure mode `AGENTS.md`'s merge-confirm playbook already documents
for interactive sessions, just unhandled at the bot-workflow level. Found and worked around by hand
(reviewed + `gh pr merge --auto` re-run) while unblocking `#624`'s merge, whose build depended on the
platform pin this sync PR carried.

**Resolves when:** the "Enable auto-merge" step in `platform-sync.yml` retries `gh pr merge --auto`
a few times with backoff before failing the job, and/or `platform-sync-alert.yml` (or a new lightweight
check) also flags a sync PR that's been green for more than a few minutes with no `autoMergeRequest`
set, not just a red one.

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
