# Agent configuration

This folder holds Concertable's repo-local agent skills. They are written against `AGENTS.md` and
should stay agent-agnostic: no Claude-specific tool names and no missing workflow surfaces.

Codex discovers the committed payload at `.agents/skills`, with generic standards under `.agents/standards`.
The copies supply the Concertable,
generic .NET, and generic React standards under distinct names where their upstream plugin names
would otherwise collide. They are runtime artifacts only: update their source in the shared
standards repositories, then refresh this committed payload deliberately. Codex never needs a
global Agent Standards marketplace, plugin cache, hook registration, or user skill catalogue.

Run `scripts/verify-agent-standards.ps1` to prove the local payload, workflow agents, hook, and
Agent Workboard isolation. Add `-VerifyFreshCodexSession -CodexExecutable <path>` to launch Codex
with `--ignore-user-config` and prove the project payload is sufficient on its own.

## Adding or updating skills

The shared canonical skills remain in their owning standards repositories. Do not hand-edit the
committed delivery copies under `.agents/skills`; refresh them together from their sources.

If Claude Code also needs to discover the skill, add only a compatibility stub at
`.claude/skills/<name>/SKILL.md` that points back to the matching `.agents` skill. Do not duplicate
the full instructions in `.claude`; duplicated skill bodies drift.

## Hooks are all vendored — change them upstream, never here

Every file in `.agents/hooks/` is **generated** from `Concertable/agent-standards` and listed in
`vendored.json`. Change it there, then re-sync with that repo's
`.agents/vendor-hooks.ps1 -Into <this repo>`; editing a copy in place fails `test_vendored_hooks.py`.
Each hook enforces a standard that lives upstream too, so the rule and the thing that enforces it
cannot drift apart.

What stays here is this repo's **data**: `.agents/skill-routes.json` (path → owning standard) and
`.agents/merge-gate.json` (this repo's security-sensitive paths). A hook without its table does
nothing; the table without the hook enforces nothing. Tests split the same way — the mechanism is
tested upstream over fixtures, and `tests/` here only asserts that this repo's own tables produce the
verdicts we expect.

A hook is carried rather than installed on purpose: a plugin only runs on a machine where someone
installed it, so enforcement that depends on an install is absent on a fresh clone. Each manifest entry
carries the `delivery` upstream derived from its own wiring: a `hook` fires from a harness event and is
wired in both `.claude/settings.json` and `.codex/hooks.json` — one harness only is the defect, not a
partial rollout — and an `invoked` one is a command-line check or a file another hook runs by path, so
it is wired in neither. `tests/test_vendored_hooks.py` asserts both directions; treating "wired
nowhere" as legal on its own is what let a harness-fired hook lose both wirings and stay green. The
single exception is `merge_review_gate.py`, which knows only Claude's `Bash` tool name; `SINGLE_HARNESS`
in that file carries the exemption and its reason, and the suite fails if the exemption outlives it.

## Global starter kit

Concertable skills belong in this repo. Do not mirror them into a global skills repo.

For personal/global skills that should follow Tommy between machines, use a separate starter-kit repo
with this shape:

```text
agent-starter-kit/
  .agents/
    skills/
      worktree/
      sync/
      ...
```

The bare `worktree` and `sync` names belong to that personal set. The repository-workflow procedures of
the same shape ship from `agent-process` as `open-worktree` and `sync-checkout`, renamed precisely so a
personal skill and a plugin skill cannot resolve under one name and shadow each other.
