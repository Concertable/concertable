# Agent configuration

This folder holds Concertable's repo-local agent skills and canonical command instructions. They are
written against `AGENTS.md` and should stay agent-agnostic: no Claude-specific tool names and no
missing workflow surfaces.

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

Canonical command instructions live under `.agents/commands/`. Claude and Codex command entry points
must be thin wrappers that reference the matching canonical file so their behavior cannot drift.

Codex discovers those wrappers through the repo's `concertable` plugin under
`plugins/concertable/commands/`. Install the repo marketplace once, then install the plugin:

```powershell
codex plugin marketplace add <repo-root>
codex plugin add concertable@concertable
```
