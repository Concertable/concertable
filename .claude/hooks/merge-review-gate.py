r"""PreToolUse hook: no `gh pr merge` without a current, clean code-review.

Blocks a Bash tool call that would merge / enable auto-merge on a PR unless a
review work-order exists for the current branch, is stamped at the exact commit
being merged, and has zero open `- [ ]` findings. This is the *enforced* form of
the AGENTS.md rule "do a code-review before you merge" — advisory prose was
ignored once; a hook cannot be.

Scope: only fires on commands containing `gh pr merge`. `--disable-auto` (turning
a merge OFF) is always allowed — it is the safe direction. Every other merge form
(`--auto`, `--admin`, `--merge`, `--squash`, `--rebase`, bare) must pass the gate.

Contract: exit 0 = allow; exit 2 = block (stderr is fed back to the agent). For a
merge command the hook fails CLOSED — any doubt (missing/stale/unclean review, or
even an internal error) blocks, because letting an unreviewed merge through is the
exact failure this guard exists to prevent. Non-merge commands always exit 0.

The review file is what `code-review` / `docs-review` write:
`reviews/<branch-with-slashes-as-dashes>.md`, carrying a top-of-file
`**Reviewed up to commit:** \`<sha>\`` marker and `- [ ]` / `- [x]` findings.
"""

import json
import re
import subprocess
import sys


def git(*args):
    return subprocess.run(
        ["git", *args], capture_output=True, text=True, check=True
    ).stdout.strip()


def is_merge_enable(command):
    if "gh pr merge" not in command:
        return False
    # Turning auto-merge OFF is the safe direction — never gate it.
    return "--disable-auto" not in command


def block(reason):
    sys.stderr.write(reason)
    sys.exit(2)


def main():
    try:
        data = json.load(sys.stdin)
    except ValueError:
        sys.exit(0)  # not our JSON — don't interfere

    if data.get("tool_name") != "Bash":
        sys.exit(0)

    command = (data.get("tool_input") or {}).get("command", "")
    if not isinstance(command, str) or not is_merge_enable(command):
        sys.exit(0)

    # From here on the command WOULD merge — fail closed on anything unproven.
    try:
        branch = git("rev-parse", "--abbrev-ref", "HEAD")
        head = git("rev-parse", "HEAD")
        toplevel = git("rev-parse", "--show-toplevel")
    except Exception as exc:  # noqa: BLE001 — a merge with a broken check must not slip through
        block("MERGE GATE: cannot resolve git state (" + str(exc) + "); refusing "
              "`gh pr merge` until a code-review can be verified.")

    slug = branch.replace("/", "-")
    review_path = toplevel + "/reviews/" + slug + ".md"

    try:
        with open(review_path, encoding="utf-8") as fh:
            review = fh.read()
    except OSError:
        block(
            "MERGE GATE (AGENTS.md — review before merge): no review file for "
            "branch '" + branch + "' at reviews/" + slug + ".md. Run /code-review "
            "(or /docs-review for a docs-only branch) and address findings, THEN "
            "merge. Do NOT merge unreviewed."
        )

    m = re.search(r"Reviewed up to commit:.*?`([0-9a-fA-F]{7,40})`", review)
    if not m:
        block("MERGE GATE: reviews/" + slug + ".md has no `Reviewed up to commit:` "
              "marker. Re-run /code-review to stamp it, then merge.")

    reviewed = m.group(1).lower()
    if not (head.lower().startswith(reviewed) or reviewed.startswith(head.lower())):
        block(
            "MERGE GATE: review is STALE — reviews/" + slug + ".md is stamped at "
            + reviewed + " but HEAD is " + head[:12] + ". Commits landed since the "
            "review. Re-run /incremental-review (or /code-review), then merge."
        )

    open_findings = [
        ln.strip() for ln in review.splitlines()
        if re.match(r"^\s*-\s*\[\s\]", ln)
    ]
    if open_findings:
        block(
            "MERGE GATE: review has " + str(len(open_findings)) + " OPEN finding(s) "
            "in reviews/" + slug + ".md. Address (or explicitly [wontfix]) every "
            "`- [ ]` item, then merge. First: " + open_findings[0][:120]
        )

    sys.exit(0)  # reviewed, current, clean → allow the merge


if __name__ == "__main__":
    main()
