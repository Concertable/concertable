# Agent configuration

This folder holds Concertable's repo-local agent skills. They are written against `AGENTS.md` and
should stay agent-agnostic: no Claude-specific tool names and no missing workflow surfaces.

## Adding or updating skills

The canonical skill always lives under `.agents/skills/<name>/SKILL.md`.

If Claude Code also needs to discover the skill, add only a compatibility stub at
`.claude/skills/<name>/SKILL.md` that points back to the matching `.agents` skill. Do not duplicate
the full instructions in `.claude`; duplicated skill bodies drift.

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

`pull-main` is intentionally not part of the global set anymore. `sync` covers the useful default
branch update flow.

## Invoking repository skills

Codex discovers `.agents/skills/` automatically; repository skills do not need a plugin or marketplace.
In the ChatGPT desktop app, type `@` and select the skill. In Codex CLI, type `$` or use `/skills`.

Claude slash-command wrappers under `.claude/commands/` must remain thin references to the canonical
skill so the workflow cannot drift between agents.
