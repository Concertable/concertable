---
name: after-merge
description: Watch the current branch's PR until another Claude instance merges it and syncs master, then immediately start the NEXT feature on a fresh branch off clean master — unattended. Takes the next feature as its argument (`/after-merge <what to build next>`). Use when Tommy wants to queue up follow-on work behind a merge someone else is driving: "after merge do X", "once this lands start on Y", "when master's synced build Z", "queue up the next feature". Concertable-specific (knows this repo's merge queue + branch conventions).
---

# after-merge

One command to **wait out the current merge and roll straight into the next feature**: watch the
current branch's PR until it merges and `master` is synced, then branch off clean `master` and start
building whatever `PROMPT` describes — no babysitting and no manual context switch.

Invoked as **`/after-merge <PROMPT>`**, where `PROMPT` is the next feature to build once the current
one has landed.

## This skill only WATCHES — another instance does the merge + sync

The merge itself is being driven **elsewhere** — a separate Claude instance running `/merge`
enqueues the PR into the E2E merge queue, waits for it to land, and returns the repo to a clean,
pulled `master` (checkout + pull + delete the merged branch). **This skill does none of that.** It
must **not** enqueue, must **not** run `gh pr merge`, and must **not** checkout / pull / delete
anything — touching git state here would race the instance that owns it.

Its whole job is: **monitor until merged AND master is synced → then start the next feature.** Read-only
observation only (`gh pr view`, `git rev-parse`, `git merge-base`) until it's time to branch.

## Steps

1. **Read the next-feature `PROMPT` (the argument).**
   - If no `PROMPT` was given, **stop** and ask what to build next — there's nothing to queue.

2. **Identify the PR to watch (read-only).**
   ```
   gh pr view --json number,state,headRefName,mergeCommit,url --jq '{number,state,headRefName,url}'
   ```
   - Record the PR number and its head branch. If there's no PR for the current branch, **stop** and
     say so — there's nothing to wait on.
   - If it's already `MERGED`, skip to step 4. If `CLOSED` (unmerged), **stop** and report.

3. **Monitor until the PR is `MERGED`.**
   - Poll the PR state — prefer the `Monitor` tool with an until-loop so you're notified instead of
     busy-waiting (the other instance's queue runs E2E; allow ~30–40 min):
     ```
     while true; do st=$(gh pr view <n> --json state --jq .state 2>&1);
       echo "$st"; [ "$st" = "MERGED" ] && break; [ "$st" = "CLOSED" ] && { echo "CLOSED-unmerged"; break; };
       sleep 60; done
     ```
   - **Don't act on the merge** — no enqueue, no admin, no nudging the queue. If it goes `CLOSED`
     unmerged, or the queue kicks it back to `OPEN` on red E2E, **stop** and report that the merge
     stalled; the next feature waits until it actually lands. Re-run `/after-merge <PROMPT>` later.

4. **Wait until `master` is synced with the merge (the other instance does the pull).**
   - Grab the merge commit: `gh pr view <n> --json mergeCommit --jq .mergeCommit.oid`.
   - Poll (read-only) until local `master` contains it **and** you're sitting on a clean `master` — i.e.
     the other instance has finished its checkout + pull + branch cleanup. Don't pull it yourself:
     ```
     while true; do
       br=$(git rev-parse --abbrev-ref HEAD);
       if [ "$br" = "master" ] && git merge-base --is-ancestor <mergeSha> master 2>/dev/null; then echo "SYNCED"; break; fi;
       sleep 30; done
     ```
   - If after a reasonable wait `master` still hasn't picked up the merge (the other instance may have
     stopped), **stop** and say master isn't synced yet — don't branch off stale `master`.

5. **Start the next feature from clean `master`.**
   - You're on synced `master`. **Branch for the work**, per this repo's rules (`CLAUDE.md` "Git
     branch"): `<Type>/<Name>` with a **capitalized** type prefix inferred from `PROMPT` (`Feature/`,
     `Bug/`, `Fix/`, `Refactor/`, …). Match the casing of any existing branch of the same name exactly;
     never a lowercase `feature/…`.
   - **Implement `PROMPT` as a normal task** — all the usual project guidance applies (plans for
     multi-step work per `plans/CLAUDE.md`, module boundaries, seeding, C# conventions, tests, the
     `ARCHITECTURE.md` service rules for anything crossing a boundary). Design first if it's
     non-trivial; act directly if it's small.
   - **Committing / pushing the next feature:** do it **only if `PROMPT` explicitly says to** (e.g.
     "…then open a PR"). Absent that, leave the work in the working tree for Tommy to review — the
     standing rule is never to commit or push without an explicit instruction in the current message,
     and `/after-merge` only instructs you to *wait then build*, not to ship.

## Final summary

Two short parts: (1) the merge you observed — the PR that landed (number + merge commit) and that
`master` is now synced; and (2) the next feature — the branch you created and what you built. If you
stopped early, say exactly why (merge stalled / `CLOSED` / master never synced) and that the next
feature is deferred until `/after-merge <PROMPT>` is re-run.

Keep it terminal: read `PROMPT` → watch until `MERGED` → wait until `master` synced → branch + build
→ summarize → stop. Read-only `git`/`gh` only until you branch (personal repo — never the work PR/ADO
skills).
