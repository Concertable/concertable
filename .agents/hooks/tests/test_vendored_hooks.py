import hashlib
import importlib.util
import json
import unittest
from pathlib import Path


HOOKS = Path(__file__).resolve().parents[1]
REPO = HOOKS.parents[1]
MANIFEST = HOOKS / "vendored.json"
WIRING = (REPO / ".claude" / "settings.json", REPO / ".codex" / "hooks.json")

# The ONE place a half-wired hook is legal, and only with its reason written down. Each entry is
# outstanding work, not a settled shape: delete it the moment the hook can be wired everywhere.
SINGLE_HARNESS = {
    "merge_review_gate.py": (
        "the gate's SHELL_TOOLS vocabulary holds only Claude's `Bash`. Codex's shell tool name is "
        "not established, and wiring a matcher the hook ignores is enforcement that is inert while "
        "looking wired. Add the name to SHELL_TOOLS upstream, wire .codex/hooks.json, drop this row."
    ),
}


def normalized(path):
    return path.read_bytes().decode("utf-8").replace("\r\n", "\n")


def load_hook(name):
    spec = importlib.util.spec_from_file_location(Path(name).stem, HOOKS / name)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def matchers_for(wiring_path, hook_name):
    """Every matcher string on a hook entry whose command runs this hook."""
    found = []

    def walk(node):
        if isinstance(node, dict):
            inner = node.get("hooks")
            if isinstance(inner, list) and any(
                hook_name in str(entry.get("command", "")) + str(entry.get("commandWindows", ""))
                for entry in inner
                if isinstance(entry, dict)
            ):
                found.append(node.get("matcher") or "")
            for value in node.values():
                walk(value)
        elif isinstance(node, list):
            for value in node:
                walk(value)

    walk(json.loads(wiring_path.read_text(encoding="utf-8")))
    return found


class VendoredHookTests(unittest.TestCase):
    def setUp(self):
        self.entries = json.loads(MANIFEST.read_text(encoding="utf-8"))["hooks"]

    def test_the_manifest_lists_at_least_one_vendored_hook(self):
        self.assertTrue(self.entries)

    def test_every_vendored_hook_matches_the_hash_it_was_generated_with(self):
        for name, entry in self.entries.items():
            with self.subTest(hook=name):
                body = normalized(HOOKS / name)
                digest = "sha256:" + hashlib.sha256(body.encode("utf-8")).hexdigest()
                self.assertEqual(
                    entry["sha256"],
                    digest,
                    f"{name} was edited in place. It is generated from {entry['source']} - change it "
                    "there and re-run that repo's vendor-hooks.ps1, or the fix is lost on the next sync.",
                )

    def test_every_vendored_hook_records_where_it_came_from(self):
        for name, entry in self.entries.items():
            with self.subTest(hook=name):
                self.assertTrue(entry["source"])
                self.assertTrue(entry["path"])
                self.assertRegex(entry["commit"], r"^[0-9a-f]{40}$")

    def test_every_vendored_hook_is_wired_for_both_harnesses(self):
        # A hook wired in one harness only is the defect this vendoring exists to remove: the router
        # spent its first life in .claude/settings.json alone, so Codex never ran it. A hook wired in
        # NEITHER is a vendored command-line check rather than a hook, and is not that defect.
        for name in self.entries:
            if name in SINGLE_HARNESS:
                continue
            wired = [w for w in WIRING if name in w.read_text(encoding="utf-8")]
            if not wired:
                continue
            for wiring in WIRING:
                with self.subTest(hook=name, wiring=wiring.name):
                    self.assertIn(name, wiring.read_text(encoding="utf-8"))

    def test_every_single_harness_exemption_is_still_needed(self):
        # The allowlist is the one place a half-wired hook is legal, so it must not outlive its
        # reason - an exemption for a hook that is now wired everywhere hides the next regression.
        for name, reason in SINGLE_HARNESS.items():
            with self.subTest(hook=name):
                self.assertIn(name, self.entries, f"{name} is exempted but no longer vendored.")
                wired = [
                    w.parent.name for w in WIRING if name in w.read_text(encoding="utf-8")
                ]
                self.assertEqual([".claude"], wired, f"{name}: {reason}")

    def test_every_wired_tool_name_is_one_the_hook_acts_on(self):
        # Being named in both wiring files is not the same as running in both. The router was matched
        # on Codex's `apply_patch` while its own tool list held only Claude's names, so it exited 0 on
        # every Codex write - wired, tested, and enforcing nothing. Presence of the filename cannot
        # see that; agreement between the matcher and the hook's own vocabulary can.
        for name in self.entries:
            hook = load_hook(name)
            vocabulary = getattr(hook, "WRITE_TOOLS", None)
            if vocabulary is None:
                continue  # not a tool-matched hook (a Stop hook has no matcher to agree with)
            for wiring in WIRING:
                for matcher in matchers_for(wiring, name):
                    for tool in (t.strip() for t in matcher.split("|") if t.strip()):
                        with self.subTest(hook=name, wiring=wiring.name, tool=tool):
                            self.assertIn(
                                tool.lower(),
                                vocabulary,
                                f"{wiring.name} routes '{tool}' to {name}, but the hook ignores that "
                                "tool name and exits 0 - the wiring enforces nothing for it.",
                            )


if __name__ == "__main__":
    unittest.main()
