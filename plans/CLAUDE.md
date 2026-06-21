# How plans work (`plans/*.md`)

Plans are **working docs for unfinished, multi-step work** — not an archive. Git history is the
archive. A finished plan kept "for reference" is rot: it misleads the next reader into thinking the
work is still pending. This file is the workflow; the root [`CLAUDE.md`](../CLAUDE.md) carries the
short version.

## Branch first

Before any plan work, create a `Feature/<Name>` branch relevant to the plan if you're not already on one — never commit plan work to `master` or an unrelated branch.

## Shape of a plan

A plan describes a chunk of work too big for one commit, broken into **phases that are each
independently shippable and each end green**. A phase states what it changes, why, and its
verification gate. Phases sequence so that every intermediate state builds and passes.

## Lifecycle

1. **Write it** when the work spans multiple commits/PRs or needs a design decided up front.
2. **Branch, then work a phase** — on the plan's `Feature/<Name>` branch (see "Branch first"), land the phase's commit(s).
3. **Check off / strike the shipped phase in the plan, in the same commit as the work.** A
   partially-done plan stays; only the outstanding work should remain un-ticked, so the next reader
   sees exactly what's left.
4. **Delete the plan** (`git rm`) in the commit that completes its *last* phase — never defer deletion
   to a later cleanup pass.
5. A plan **superseded** by a newer plan, or describing a **rejected** design, is deleted the moment
   that's decided — no tombstones.

## End of a phase — leave it compaction/clear-safe

A phase isn't done when the code is green; it's done when **nothing important lives only in the chat
context**. After the phase's commit lands and its gate passes, get all durable state written down so
the conversation can be thrown away without losing anything:

- **Plan markdown** — checked off / struck per the Lifecycle above (or `git rm`'d if it was the last
  phase). The next reader should see exactly what's left and nothing that already shipped.
- **Memory** — if the phase changed a decision, convention, or fact worth carrying forward, update
  the relevant `CLAUDE.md` / `TECH_DEBT.md` so it survives independent of the chat.
- Anything else you'd be annoyed to re-derive (a command that worked, a gotcha hit) belongs in the
  commit message or the appropriate doc — not left implicit in the context.

Most of the time the context is then **compacted** (summarized, work continues) — but the same prep
makes it safe to **clear** (start fresh) instead. Either way, treat the end of every phase as the
point where the context becomes disposable. Don't carry unwritten state across a phase boundary.

## Verification gate per phase

Every phase, no exceptions:

- `dotnet build api/Concertable.slnx` green (0 errors).
- The **affected** module's unit + integration tests — run them via the `integration-debug` skill.
- Phases that change the model end with `./initial-migrations.ps1` from `api/` (re-scaffold, never
  additive migrations).
- **Final phase only:** run the UI E2E suite via the `e2e-ui-debug` skill.

## A failing test is never just reported — enter the matching debug skill and drive it to green

Whenever **any** test run comes back red — unit, integration, API E2E, or UI E2E — the next action is
**always** the matching debug skill, not a status report back to the user. A failure *is* the trigger;
don't make the user ask for it.

- unit / integration failures → **`integration-debug`**
- API E2E failures → **`e2e-api-debug`**
- UI E2E failures → **`e2e-ui-debug`** (or **`e2e-debug`** to sweep both layers)

The debug skill owns the whole **run → diagnose → fix → re-run** loop: find the root cause, fix it in
code (service / handler / page-object / step-def / fixture / config — wherever the real bug is), and
re-run until green. Never report a red suite and wait. Never disable, skip, `--no-build`, or
inflate-a-timeout to get past a failure — that's bypassing, not fixing. For E2E, the debug skill also
does **flaky-vs-real triage**: re-run the failed scenario alone on a fresh stack — passes clean = a
host-load blip (proven, not assumed); fails again = a real bug, so fix it.

## When to run the E2E suites — judgment, not reflex

The full E2E suites (API `Concertable.B2B.E2ETests` + the UI regress) are **expensive and
Docker-gated**. Run them only when the change earns it; otherwise build + unit + integration is the
gate, and you update the plan markdown and move on.

**Run E2E when the change is _massive_ or _risky_:**

- It spans multiple services or is otherwise broadly cross-cutting.
- It changes **user-facing or runtime behavior** in a flow E2E covers — registration/login, payments
  & payouts, settlement, the event/projection chain, messaging.
- It's the kind of change that's **likely to break something and you'd want to debug it first** —
  i.e. you're not confident unit + integration fully covers the blast radius.

**Skip E2E (just build + unit + integration, update the markdown, continue) when:**

- It's foundational / stage-1 implementation with **zero behavior change** (a new table + seam that
  nothing exercises yet).
- It's small, isolated, or covered well by integration tests.
- It's doc-only or comments-only.

When in doubt, or when a phase explicitly flips behavior on a covered flow, run E2E. **How** to run it
safely (the mandatory `./docker-health.ps1` pre-flight, only via the `e2e-*` skills) is unchanged —
see the "E2E suites — Docker health first" section in `CLAUDE.md`. This section governs **whether**,
that one governs **how**.

A phase's own "verification gate" line may name E2E; treat that as "run E2E *if* this phase meets the
massive/risky bar above," not as an unconditional requirement for every phase.
