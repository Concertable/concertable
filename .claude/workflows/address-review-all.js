export const meta = {
  name: 'address-review-all',
  description: 'Work through every OPEN finding in a code-review file one at a time, each in its own fresh subagent context: fix clear defects (commit per finding, no push), defer judgment calls, then delete the review if every finding was fixed cleanly.',
  whenToUse: 'Address all the findings in a review file unattended. Pass the review-file path as args; omit to auto-detect the current branch\'s review with open findings.',
  phases: [{ title: 'Fix' }, { title: 'Finalize' }],
}

// Sequential ONE-subagent-per-finding loop. Strictly sequential (never parallel) because each
// subagent edits the working tree, verifies, and commits: a fresh clean tree per finding means
// fixes can't clobber each other, and each finding lands as its own revertible commit.
//
// Policy (chosen by the user, encoded here):
//  - Commit per finding, NEVER push.
//  - Fix only CLEAR defects; DEFER judgment-call / subjective findings (mark them, don't touch code).
//  - Delete the review file at the end ONLY if every finding was actually FIXED (nothing deferred).
//
// No args needed: the first subagent auto-detects the review file and threads it through the loop.

const pinnedFile = typeof args === 'string' && args.trim() ? args.trim() : null

const FIX_SCHEMA = {
  type: 'object',
  additionalProperties: false,
  required: ['reviewFile', 'findingId', 'action', 'remainingOpen'],
  properties: {
    reviewFile: { type: ['string', 'null'], description: 'Repo-relative path of the review file operated on (detected or pinned). null only if no review file with open findings exists.' },
    findingId: { type: ['string', 'null'], description: 'The finding handled this run (e.g. "BUG1"), or null if none was open.' },
    action: { type: 'string', enum: ['fixed', 'deferred', 'none'], description: 'fixed = clear defect resolved + committed; deferred = judgment call left for a human, code untouched; none = no open finding remained.' },
    summary: { type: 'string', description: 'One line: what was fixed, or why it was deferred.' },
    filesTouched: { type: 'array', items: { type: 'string' }, description: 'Code files changed (empty for deferred / already-resolved).' },
    committed: { type: 'boolean', description: 'Whether a commit was made this run.' },
    remainingOpen: { type: 'integer', description: 'Count of "- [ ]" finding items STILL open in the review file after this run.' },
  },
}

const FINAL_SCHEMA = {
  type: 'object',
  additionalProperties: false,
  required: ['buildPassed', 'reviewDeleted', 'deferredCount'],
  properties: {
    buildPassed: { type: 'boolean', description: 'Did the full solution build pass after all fixes?' },
    reviewDeleted: { type: 'boolean', description: 'Was the review file git-rm\'d (only when every finding was FIXED and the build is green)?' },
    deferredCount: { type: 'integer', description: 'Number of findings left deferred for a human decision.' },
    deferred: { type: 'array', items: { type: 'string' }, description: 'The deferred finding IDs + one-line reason each.' },
    notes: { type: 'string', description: 'Anything the user must know (build failure, what a deferred finding needs, etc.).' },
  },
}

function fileClause(resolvedFile) {
  if (resolvedFile) {
    return 'The review file is EXACTLY "' + resolvedFile + '" — operate on that file only.'
  }
  return [
    'DETECT the review file yourself:',
    '   - Glob reviews/*.md (ignore reviews/CLAUDE.md). Keep the files whose slug matches the CURRENT git branch (branch "/" -> "-").',
    '   - Among those, the target is the one with open findings — lines matching "- [ ] **<ID>" under its ## Findings section. Prefer the highest wave suffix / most recently modified if several match.',
    '   - If none has an open finding, report reviewFile = the best matching file (or null if none exists), action="none", findingId=null, remainingOpen=0.',
    '   - Report the path you chose as reviewFile so the loop reuses it.',
  ].join('\n')
}

function fixPrompt(n, resolvedFile) {
  return [
    'You are addressing ONE finding from a code review, in a fresh isolated context. This is run #' + n + ' of a sequential loop that keeps invoking a new context per finding until none are open. The review file on disk is the shared state.',
    '',
    fileClause(resolvedFile),
    '',
    'Procedure:',
    '1. Read `reviews/CLAUDE.md`, `.claude/skills/address-review/SKILL.md`, and the review file in full. Also read `CLAUDE.md` / the nearest area `CLAUDE.md`, `api/agents/CODE_PATTERNS.md`, and the relevant `CODE_CONVENTIONS.md` before changing code.',
    '2. Pick the NEXT open finding: the first line matching "- [ ] **<ID>" under ## Findings. If there is none, report action="none", findingId=null, remainingOpen=0, and stop.',
    '3. Read that finding fully — file:line, description, severity, recommended fix, and ANY counter-argument it states.',
    '4. Classify it:',
    '   - CLEAR DEFECT with an unambiguous fix (a correctness bug, a microservice-isolation / module-boundary violation, a seeding error, or a convention nit with a stated fix) → FIX it.',
    '   - JUDGMENT CALL — the finding is framed as a tradeoff / "author\'s call", is subjective, presents a real counter-argument for NOT changing it, gives no clear recommended action, OR you are not highly confident it is a real defect with a safe fix → DEFER (do NOT change code). Never silently dismiss a finding as "not real" — deferring surfaces it for a human; dismissing buries it.',
    '5. If FIX:',
    '   a. Make the MINIMAL correct change, matching surrounding code and repo conventions. Do not fix anything the finding did not name.',
    '   b. Verify: build the affected project(s) and run the NEAREST unit/integration tests for what you touched. Do NOT run E2E. If verification fails and you cannot quickly resolve it, REVERT your change and DEFER instead (say why in summary).',
    '   c. Tick the finding: change its "- [ ]" to "- [x]" and append " — FIXED (<one-line what>)". Resolve any related note.',
    '   d. Commit ONLY the files you touched PLUS the review file, using an explicit pathspec — do NOT `git add -A` / `git add .` (the working tree contains unrelated changes that must NOT be swept in). Conventional message referencing the finding ID, no push:',
    '      git commit <touched-file> [...] <review-file> -m "<type>(<scope>): <ID> — <desc>" -m "Co-Authored-By: Claude Opus 4.8 <noreply@anthropic.com>"',
    '6. If DEFER: change that finding\'s "- [ ]" to "- [-]" and append " — DEFERRED: <what decision is needed>". Do NOT edit code. Commit ONLY the review file (pathspec-scoped, "docs(review): defer <ID>" + the Co-Authored-By trailer) so loop state persists.',
    '7. Report: reviewFile, findingId, action, summary, filesTouched, committed, and remainingOpen = count of "- [ ]" finding items still open AFTER your edit.',
    '',
    'Hard constraints: exactly ONE finding; NEVER push; NEVER run E2E; pathspec-scoped commits only (never stage unrelated changes); you own the checkbox edits.',
  ].join('\n')
}

function finalizePrompt(resolvedFile) {
  return [
    'All findings in the review file "' + resolvedFile + '" have been handled (each is now "- [x]" fixed or "- [-]" deferred). Finalize, in a fresh context:',
    '',
    '1. Run a FULL solution build to confirm the accumulated fixes are green (find the .sln and `dotnet build` it). Do NOT run E2E.',
    '2. Read the review file and tally: are ALL findings "- [x]" (FIXED), with ZERO "- [-]" deferred?',
    '3. If build is GREEN and every finding was FIXED (none deferred): `git rm` the review file and commit it alone — "docs(review): remove ' + resolvedFile + ' — all findings addressed" plus the "Co-Authored-By: Claude Opus 4.8 <noreply@anthropic.com>" trailer. Set reviewDeleted=true. (This follows reviews/CLAUDE.md: a review with nothing left to act on is deleted.)',
    '4. Otherwise KEEP the file (it still holds deferred items or the build failed). Do not delete. Set reviewDeleted=false.',
    '5. Never push.',
    '',
    'Report: buildPassed, reviewDeleted, deferredCount, deferred (IDs + one-line reason each), notes (call out any build failure or what each deferred finding needs).',
  ].join('\n')
}

const MAX_FINDINGS = 60 // safety backstop
const fixes = []
let resolvedFile = pinnedFile
let n = 0
let more = true

while (more && n < MAX_FINDINGS) {
  n += 1
  const r = await agent(fixPrompt(n, resolvedFile), {
    label: resolvedFile ? 'finding-' + n : 'detect+finding-' + n,
    phase: 'Fix',
    schema: FIX_SCHEMA,
    agentType: 'general-purpose',
    effort: 'high',
  })

  if (!r) {
    log('Run #' + n + ' returned no result — stopping so it can be inspected.')
    break
  }
  if (!resolvedFile && r.reviewFile) {
    resolvedFile = r.reviewFile
    log('Review file: ' + resolvedFile)
  }
  if (!resolvedFile) {
    log('No review file with open findings found for this branch — nothing to address.')
    return { reviewFile: null, findingsHandled: 0, complete: true }
  }

  if (r.action === 'none') {
    log('No open findings remain.')
    break
  }
  fixes.push(r)
  log((r.action === 'fixed' ? 'Fixed' : 'Deferred') + ' ' + r.findingId + ': ' + (r.summary || '') + ' — ' + r.remainingOpen + ' open left.')
  more = r.remainingOpen > 0
}

if (more && n >= MAX_FINDINGS) {
  log('Hit the ' + MAX_FINDINGS + '-finding safety cap — inspect the review file; a finding may be stuck re-opening.')
}

phase('Finalize')
let finalize = null
if (resolvedFile && fixes.length) {
  finalize = await agent(finalizePrompt(resolvedFile), {
    label: 'finalize',
    phase: 'Finalize',
    schema: FINAL_SCHEMA,
    agentType: 'general-purpose',
    effort: 'high',
  })
  if (finalize) {
    log('Build ' + (finalize.buildPassed ? 'green' : 'FAILED') + '; review file ' + (finalize.reviewDeleted ? 'deleted (all fixed)' : 'kept') + '; ' + finalize.deferredCount + ' deferred.')
  }
}

return {
  reviewFile: resolvedFile,
  fixed: fixes.filter((f) => f.action === 'fixed').map((f) => f.findingId),
  deferred: fixes.filter((f) => f.action === 'deferred').map((f) => f.findingId),
  buildPassed: finalize?.buildPassed ?? null,
  reviewDeleted: finalize?.reviewDeleted ?? false,
  notes: finalize?.notes ?? null,
}
