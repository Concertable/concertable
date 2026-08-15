import argparse
import json
import re
import sys
from pathlib import Path


REFERRER_NAMES = {"AGENTS.md", "CLAUDE.md", "SKILL.md"}
CLAUDE_BODY = "@AGENTS.md"
IGNORED_DIR_NAMES = {"node_modules", "bin", "obj", "dist", ".git"}
LINK_PATTERN = re.compile(r"\]\(([^)\s]+)\)")
AT_IMPORT_PATTERN = re.compile(r"(?<![\w@])@([\w./-]+\.md)")


def ignored(relative_path):
    return any(part in IGNORED_DIR_NAMES for part in relative_path.parts[:-1])


def hidden(relative_path):
    return any(part.startswith(".") for part in relative_path.parts[:-1])


def agents_md_files(root):
    return sorted(
        path
        for path in root.rglob("AGENTS.md")
        if not hidden(path.relative_to(root)) and not ignored(path.relative_to(root))
    )


def agents_dir_docs(root):
    return sorted(
        path
        for path in root.rglob("agents/*.md")
        if not hidden(path.relative_to(root)) and not ignored(path.relative_to(root))
    )


def all_md_files(root):
    return sorted(path for path in root.rglob("*.md") if not ignored(path.relative_to(root)))


def resolve_reference(referrer, raw_ref):
    ref = raw_ref.strip().split("#", 1)[0].strip()
    if not ref or ref.startswith(("http://", "https://", "mailto:")):
        return None
    try:
        return (referrer.parent / ref).resolve()
    except OSError:
        return None


def references_in(path):
    text = path.read_text(encoding="utf-8")
    targets = set()
    for pattern in (LINK_PATTERN, AT_IMPORT_PATTERN):
        for match in pattern.finditer(text):
            target = resolve_reference(path, match.group(1))
            if target:
                targets.add(target)
    return targets


def sibling_errors(root):
    errors = []
    for path in agents_md_files(root):
        claude = path.with_name("CLAUDE.md")
        if not claude.is_file():
            errors.append(
                f"{path.relative_to(root)}: has no sibling CLAUDE.md "
                f"(every AGENTS.md must have a CLAUDE.md containing exactly `{CLAUDE_BODY}`)"
            )
            continue
        body = claude.read_text(encoding="utf-8").strip()
        if body != CLAUDE_BODY:
            errors.append(
                f"{claude.relative_to(root)}: must contain exactly `{CLAUDE_BODY}`, found `{body}`"
            )
    return errors


def reachable_docs(root):
    files = all_md_files(root)
    edges = {path.resolve(): references_in(path) for path in files}
    queue = [path.resolve() for path in files if path.name in REFERRER_NAMES]
    visited = set()
    while queue:
        current = queue.pop()
        if current in visited:
            continue
        visited.add(current)
        queue.extend(edges.get(current, ()))
    return visited


def orphan_errors(root):
    reachable = reachable_docs(root)
    errors = []
    for doc in agents_dir_docs(root):
        if doc.resolve() not in reachable:
            errors.append(
                f"{doc.relative_to(root)}: not reachable (by plain link or @-import, followed "
                "transitively) from any AGENTS.md, CLAUDE.md, or SKILL.md in the repository"
            )
    return errors


def repository_report(root):
    errors = sibling_errors(root) + orphan_errors(root)
    return {"errors": errors, "warnings": []}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path.cwd())
    parser.add_argument("--json", action="store_true")
    args = parser.parse_args()
    report = repository_report(args.root.resolve())
    if args.json:
        json.dump(report, sys.stdout, indent=2)
        sys.stdout.write("\n")
    else:
        for level in ("errors", "warnings"):
            for message in report[level]:
                print(f"{level[:-1].upper()}: {message}")
        print(f"Docs reachability: {len(report['errors'])} error(s), {len(report['warnings'])} warning(s)")
    raise SystemExit(1 if report["errors"] else 0)


if __name__ == "__main__":
    main()
