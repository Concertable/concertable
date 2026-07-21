export const meta = {
  name: 'big-review-all',
  description: 'Drive every remaining big-review stage to completion, each stage in its own fresh subagent context (the automated equivalent of /big-review -> /clear -> /big-review). Auto-detects the active tracking file; no arguments needed.',
  whenToUse: 'Run the whole staged big-review unattended. Just run it — the first subagent finds the active tracking file for the current branch. Optionally pass a specific tracking-file path as args to override detection.',
  phases: [{ title: 'Stages' }],
}

// Each iteration spawns ONE subagent = ONE clean context, and each just runs the big-review
// skill for a single stage. We loop STRICTLY SEQUENTIALLY (never parallel): stages share one
// tracking file and earlier stages leave Cross-area notes that later stages must check, so
// concurrency would both race on the file and skip those hand-offs.
//
// No args required. The FIRST subagent detects the active tracking file (the current branch's
// BIG-*Review*.md that still has unticked areas -- i.e. the latest wave) and returns its path;
// the loop threads that path into later runs so detection happens exactly once. Pass args to
// pin a specific file and skip detection.

const pinnedFile = typeof args === 'string' && args.trim() ? args.trim() : null

const STAGE_SCHEMA = {
  type: 'object',
  additionalProperties: false,
  required: ['trackingFile', 'areaReviewed', 'remaining', 'allDone'],
  properties: {
    trackingFile: {
      type: ['string', 'null'],
      description: 'Repo-relative path of the tracking file this run operated on (the file you detected or were given). null only if no big-review tracking file exists at all.',
    },
    areaReviewed: {
      type: ['string', 'null'],
      description: 'Name of the area reviewed this run, or null if no area was pending (nothing to do).',
    },
    findingsAdded: {
      type: 'integer',
      description: 'Number of findings appended for this area (0 if none).',
    },
    remaining: {
      type: 'integer',
      description: 'Count of areas still [ ] or [~] in the tracking file AFTER this run.',
    },
    allDone: {
      type: 'boolean',
      description: 'True iff every area in the tracking file is now [x] (staging pass complete).',
    },
    completionStamped: {
      type: 'boolean',
      description: 'True if this run was the one that completed the pass and stamped the Summary + "Reviewed up to commit:" marker.',
    },
  },
}

function fileClause(resolvedFile) {
  if (resolvedFile) {
    return `The tracking file for this review is EXACTLY \`${resolvedFile}\` — operate on that file. Do NOT detect or derive a different one.`
  }
  return `DETECT the active tracking file yourself (do not just derive \`BIG-<slug>-Review.md\` — this branch may have wave-suffixed continuations like \`-Wave2.md\`):
   - Glob \`reviews/BIG-*Review*.md\`. Reduce to the files whose slug matches the CURRENT git branch (branch \`/\` -> \`-\`).
   - Among those, the ACTIVE one is the file that still has \`[ ]\` or \`[~]\` areas in its ## Coverage checklist. If several match a branch, prefer the highest wave suffix (\`-Wave3\` > \`-Wave2\` > base) with pending areas.
   - If every matching file is fully \`[x]\`, there is nothing to do — report trackingFile = the newest matching file, areaReviewed=null, remaining=0, allDone=true.
   - Report the path you chose as \`trackingFile\` so the outer loop reuses it.`
}

function stagePrompt(n, resolvedFile) {
  return `You are executing ONE stage of a staged "big review", in a fresh isolated context. This is run #${n} of an automated loop that keeps invoking a new context per stage until the review is complete — the on-disk tracking file's coverage checklist is the ONLY shared state, so everything you need is on disk.

${fileClause(resolvedFile)}

Do this and nothing else:

1. Read \`.claude/skills/big-review/SKILL.md\` and \`.claude/skills/code-review/SKILL.md\` in full. They are the procedure. Follow them faithfully.
2. Open the tracking file (detected per above, or the pinned path). Look at the ## Coverage checklist and pick EXACTLY ONE area to review this run:
   - If any area is \`[~]\` (a prior run died mid-stage), take the FIRST such area. Its existing findings section is untrusted — re-review from scratch and REPLACE that partial section (big-review Step 2 / Step 4).
   - Otherwise take the FIRST \`[ ]\` area.
   - If EVERY area is already \`[x]\`: do NOT re-review anything. If the completion actions (Summary rollup at top + the \`**Reviewed up to commit:**\` marker) are missing, perform them now per big-review Step 2 completion; then report areaReviewed=null, remaining=0, allDone=true.
3. Review that ONE area following big-review Step 3 (path-scoped net diff \`merge-base..HEAD\` filtered to the area's globs, skip move-only files) + code-review Steps 2-4 verbatim (rule docs, all five lenses, >=80-confidence filter). Read any Cross-area note targeting this area's paths first — those are mandatory checks. Add new Cross-area notes for things a LATER stage must verify.
4. Append findings per big-review Step 4 (continue the existing finding-ID scheme, no renumbering; "No issues found in this area" if clean), resolve any cross-area notes you checked, and flip this area's checklist item to \`[x]\` with today's date. PRESERVE every prior area's section and marks — never overwrite, except the partial \`[~]\` section you re-reviewed.
5. If flipping this area to \`[x]\` makes ALL areas \`[x]\`, perform the big-review Step 2 completion actions: write the \`## Summary\` rollup at the top and stamp the \`**Reviewed up to commit:** \\\`<full plan-anchor SHA>\\\`\` marker (use the PLAN-ANCHOR SHA from the file, not current HEAD). Set completionStamped=true.

Constraints:
- Review ONE area only. Do not run ahead into other areas' paths.
- Do NOT commit, push, or touch git state. Only edit the tracking markdown file.
- You OWN the checklist edits — the file is the source of truth for the outer loop.

Always return \`trackingFile\` (the path you operated on), plus areaReviewed, findingsAdded, remaining (count of \`[ ]\`+\`[~]\` left after your edit), allDone, completionStamped.`
}

const MAX_STAGES = 40 // safety backstop; real reviews have well under this many areas
const results = []
let resolvedFile = pinnedFile // null => first subagent detects it and reports it back
let done = false
let n = 0

while (!done && n < MAX_STAGES) {
  n += 1
  const r = await agent(stagePrompt(n, resolvedFile), {
    label: resolvedFile ? `stage-${n}` : `detect+stage-${n}`,
    phase: 'Stages',
    schema: STAGE_SCHEMA,
    agentType: 'general-purpose',
    effort: 'high',
  })

  if (!r) {
    log(`Run #${n} returned no result (subagent skipped or died) — stopping the loop so it can be inspected.`)
    break
  }

  results.push(r)
  if (!resolvedFile && r.trackingFile) {
    resolvedFile = r.trackingFile
    log(`Active tracking file: ${resolvedFile}`)
  }
  if (!r.trackingFile && !resolvedFile) {
    log('No big-review tracking file found for this branch — nothing to run. Start one with /big-review first.')
    break
  }

  done = r.allDone === true

  if (r.areaReviewed) {
    log(`Reviewed "${r.areaReviewed}" (+${r.findingsAdded ?? 0} findings). ${r.remaining} area(s) remaining.`)
  } else {
    log(`No pending area found on run #${n} — nothing to review.`)
  }
  if (r.completionStamped) log('Staging pass completed — Summary + "Reviewed up to commit:" marker stamped.')
}

if (!done && n >= MAX_STAGES) {
  log(`Hit the ${MAX_STAGES}-stage safety cap without completing — inspect the tracking file; there may be more areas than expected or a stuck [~] stage.`)
}

return {
  trackingFile: resolvedFile,
  stagesRun: results.length,
  areasReviewed: results.map((r) => r?.areaReviewed).filter(Boolean),
  complete: done,
}
