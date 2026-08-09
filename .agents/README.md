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

Canonical command instructions live under `.agents/commands/`. Agent-specific command entry points
must be thin wrappers that reference the matching canonical file so their behavior cannot drift.

Claude discovers its wrapper under `.claude/commands/`. Codex plugin commands are not a command
surface: the plugin loader migrates them into `source-command-*` skills. Keep Codex command wrappers
under `.codex/prompts/` and copy them into the user prompt directory:

```powershell
New-Item -ItemType Directory -Force "$env:USERPROFILE\.codex\prompts" | Out-Null
Copy-Item ".codex\prompts\*.md" "$env:USERPROFILE\.codex\prompts\" -Force
```

Codex exposes these custom commands under the `/prompts:` namespace; `techdebt.md` is invoked as
`/prompts:techdebt`.
