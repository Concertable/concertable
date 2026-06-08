---
name: auto-memory
description: Toggle Claude Code's auto-memory feature on or off for this project by flipping the `autoMemoryEnabled` key in `.claude/settings.local.json`. Pure toggle — no arguments. Use whenever the user wants to turn memory on/off, disable memory, enable memory, or stop/start memory recall for the project.
---

# auto-memory

Pure toggle for the project's auto-memory feature. Flips `autoMemoryEnabled` in
`.claude/settings.local.json` and reports the new state. No arguments.

The `/memory` command's UI in this build only *displays* the auto-memory status — it has
no selectable toggle — so this skill is the fast path for switching it.

## Steps

1. Read `.claude/settings.local.json`.
2. Determine current state of `autoMemoryEnabled`:
   - key present and `true` → currently ON
   - key present and `false` → currently OFF
   - key absent → auto-memory defaults to ON, so treat as ON
3. Flip it with a single Edit:
   - If currently ON → set `"autoMemoryEnabled": false`
   - If currently OFF → set `"autoMemoryEnabled": true`
   - If the key is absent, add `"autoMemoryEnabled": false` as the last key in the root
     object (insert a comma after the current last key).
4. Report the result in one line, e.g. `Auto-memory: ON → OFF`.

## Notes

- This is a settings change; it takes full effect on the next session launch. Tell the
  user that in the one-line report if you just turned it OFF mid-session.
- This only controls auto-memory (recall + saving of `MEMORY.md` / `memory/` files). It does
  **not** affect `CLAUDE.md` project instructions, which always load.
- Keep it terminal: do the edit, report the new state, stop. No preamble, no summary.
