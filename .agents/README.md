# Agent skills

This folder holds Concertable's repo-local agent skills. They are written against `AGENTS.md`
instructions and should stay agent-agnostic: no Claude-specific tool names, no missing workflow
surfaces, and no runtime-only slash command assumptions.

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

## Repository slash commands

Codex slash commands live in the repo's `concertable` plugin under
`plugins/concertable/commands/`; a bare `.agents/commands/` directory is not a registered command
surface. Install the repo marketplace once, then install the plugin:

```powershell
codex plugin marketplace add <repo-root>
codex plugin add concertable@concertable
```
