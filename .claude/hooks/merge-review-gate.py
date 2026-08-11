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

The review file is what `review` / `docs-review` write:
`reviews/<branch-with-slashes-as-dashes>.md`, carrying a top-of-file
`**Reviewed up to commit:** \`<sha>\`` marker and `- [ ]` / `- [x]` findings.

Security layer: when the reviewed range touches security-sensitive paths (Auth,
Payment, *.Contracts, controllers, auth/authz/secret/credential files, CI
workflows), the merge also requires a current `**Security-reviewed up to commit:**
\`<sha>\`` marker — stamped by `review` Step 1d after it runs `/security-review`.
"""

import json
import re
import subprocess
import sys


def git(*args, cwd="."):
    return subprocess.run(
        ["git", "-C", cwd, *args], capture_output=True, text=True, check=True
    ).stdout.strip()


_CD_RE = re.compile(r"""\bcd\s+("[^"]*"|'[^']*'|[^\s;&|<>]+)""")


def merge_target_dir(command, data):
    merge_pos = command.find("gh pr merge")
    prefix = command if merge_pos < 0 else command[:merge_pos]
    target = None
    for m in _CD_RE.finditer(prefix):
        target = m.group(1)
        if len(target) >= 2 and target[0] == target[-1] and target[0] in "\"'":
            target = target[1:-1]
    if target:
        return target
    return data.get("cwd") or "."


_ENABLE_TOKENS = ("--auto", "--admin", "--merge", "--squash", "--rebase")

# Paths whose change makes a diff security-sensitive — a merge touching any of
# these needs a current `Security-reviewed up to commit:` marker too. Kept
# targeted (concrete high-value areas) so unrelated merges are never blocked.
_SECURITY_PATTERNS = (
    re.compile(r"(^|/)Concertable\.Auth"),
    re.compile(r"(^|/)Concertable\.Payment"),
    re.compile(r"\.Contracts(/|\.)"),
    re.compile(r"Controller[A-Za-z]*\.cs$"),
    re.compile(r"^\.github/workflows/"),
    re.compile(r"(?i)(authoriz|authentic|credential|\bsecret|password|apikey|api_key)"),
)


def touches_security(changed_paths):
    for path in changed_paths:
        for pat in _SECURITY_PATTERNS:
            if pat.search(path):
                return path
    return None


def is_merge_enable(command):
    if "gh pr merge" not in command:
        return False
    # An enabling form anywhere gates — even in a compound that disables
    # auto-merge FIRST (`--disable-auto && ... --auto`, the documented re-assert
    # remedy). A whole-command "--disable-auto is present" test would fail open
    # on exactly that compound, so check for an enabling token independently.
    if any(tok in command for tok in _ENABLE_TOKENS):
        return True
    # A pure --disable-auto (no enabling token) is the safe direction — allow.
    if "--disable-auto" in command:
        return False
    # Bare `gh pr merge` with neither is still a merge attempt — gate it.
    return True


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
    target = merge_target_dir(command, data)
    try:
        branch = git("rev-parse", "--abbrev-ref", "HEAD", cwd=target)
        head = git("rev-parse", "HEAD", cwd=target)
        toplevel = git("rev-parse", "--show-toplevel", cwd=target)
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
            "branch '" + branch + "' at reviews/" + slug + ".md. Run /review "
            "(or /docs-review for a docs-only branch) and address findings, THEN "
            "merge. Do NOT merge unreviewed."
        )

    m = re.search(r"Reviewed up to commit:.*?`([0-9a-fA-F]{7,40})`", review)
    if not m:
        block("MERGE GATE: reviews/" + slug + ".md has no `Reviewed up to commit:` "
              "marker. Re-run /review to stamp it, then merge.")

    reviewed = m.group(1).lower()
    if not (head.lower().startswith(reviewed) or reviewed.startswith(head.lower())):
        block(
            "MERGE GATE: review is STALE — reviews/" + slug + ".md is stamped at "
            + reviewed + " but HEAD is " + head[:12] + ". Commits landed since the "
            "review. Re-run /incremental-review (or /review), then merge."
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

    # Security layer: a security-sensitive range also needs a current security marker.
    # Detection failure fails OPEN (the primary review gate already fired) so an
    # unresolvable base can't wedge every merge; a resolvable sensitive path fails CLOSED.
    try:
        # origin/main not local main: local main drifts stale and would false-positive
        # the security check by dragging unrelated commits into the range.
        try:
            base = git("merge-base", "origin/main", "HEAD", cwd=target)
        except Exception:  # noqa: BLE001
            base = git("merge-base", "main", "HEAD", cwd=target)
        changed = git("diff", "--name-only", base + "..HEAD", cwd=target).splitlines()
    except Exception:  # noqa: BLE001 — can't classify → don't block on the security sub-check
        changed = []

    sensitive = touches_security(changed)
    if sensitive:
        sm = re.search(r"Security-reviewed up to commit:.*?`([0-9a-fA-F]{7,40})`", review)
        if not sm:
            block(
                "MERGE GATE (security layer): '" + sensitive + "' is security-sensitive but "
                "reviews/" + slug + ".md has no `Security-reviewed up to commit:` marker. Run "
                "/security-review (or re-run /review, which runs it on sensitive paths and "
                "stamps the marker), THEN merge."
            )
        sreviewed = sm.group(1).lower()
        if not (head.lower().startswith(sreviewed) or sreviewed.startswith(head.lower())):
            block(
                "MERGE GATE (security layer): security review is STALE — reviews/" + slug + ".md "
                "security marker is at " + sreviewed + " but HEAD is " + head[:12] + ". Commits "
                "landed since the security review. Re-run /security-review, then merge."
            )

    sys.exit(0)  # reviewed, current, clean → allow the merge


if __name__ == "__main__":
    main()
