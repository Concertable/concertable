import hashlib
import importlib.util
import json
import sys
import unittest
from pathlib import Path


HOOKS = Path(__file__).resolve().parents[1]
# A hook run directly (`python skill_router.py`) gets its own directory on sys.path for free - that is
# how `from hook_runtime import ...` resolves in production. Loading the same file in-process via
# importlib does not do this automatically, so `merge_review_gate.py`'s identical import raised
# ModuleNotFoundError the moment vendoring added that dependency, even though the real file beside it
# on disk was never missing.
if str(HOOKS) not in sys.path:
    sys.path.insert(0, str(HOOKS))
REPO = HOOKS.parents[1]
MANIFEST = HOOKS / "vendored.json"
WIRING = (REPO / ".claude" / "settings.json", REPO / ".codex" / "hooks.json")
# Upstream derives these from its own hooks.json: `hook` fires from a harness event and must be wired
# in every harness here; `invoked` is run by another hook or from a command line and is wired nowhere.
DELIVERY_KINDS = ("hook", "invoked")

# The ONE place a half-wired hook is legal, and only with its reason written down. Each entry is
# outstanding work, not a settled shape: delete it the moment the hook can be wired everywhere.
SINGLE_HARNESS = {}


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
        manifest = json.loads(MANIFEST.read_text(encoding="utf-8"))
        self.entries = manifest["hooks"]
        self.scripts = manifest["scripts"]

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

    def test_every_vendored_hook_declares_how_it_is_delivered(self):
        for name, entry in self.entries.items():
            with self.subTest(hook=name):
                self.assertIn(entry.get("delivery"), DELIVERY_KINDS)

    def test_every_harness_fired_hook_is_wired_for_both_harnesses(self):
        # A hook wired in one harness only is the defect this vendoring exists to remove: the router
        # spent its first life in .claude/settings.json alone, so Codex never ran it.
        for name, entry in self.entries.items():
            if entry["delivery"] != "hook" or name in SINGLE_HARNESS:
                continue
            for wiring in WIRING:
                with self.subTest(hook=name, wiring=wiring.name):
                    self.assertIn(name, wiring.read_text(encoding="utf-8"))

    def test_an_invoked_hook_is_wired_in_no_harness(self):
        # `invoked` is a command-line check or a file another hook runs by path. Reading "wired
        # nowhere" as legal by itself is what let a harness-fired hook lose both its wirings and
        # still pass, so the kind decides and both directions are asserted.
        for name, entry in self.entries.items():
            if entry["delivery"] != "invoked":
                continue
            for wiring in WIRING:
                with self.subTest(hook=name, wiring=wiring.name):
                    self.assertNotIn(name, wiring.read_text(encoding="utf-8"))

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


class VendoredScriptTests(unittest.TestCase):
    """The second vendoring tier: repo-invariant executables, copied to the same path they hold upstream.

    They carry no `delivery`, because a script is run from a command line or by another script and never
    wired to a harness event - so the hook tier's wiring questions do not apply and a field with one
    possible value would only invite a wrong answer. What still applies is that the copy is generated:
    edit it here and the fix is lost on the next sync.
    """

    def setUp(self):
        self.scripts = json.loads(MANIFEST.read_text(encoding="utf-8"))["scripts"]

    def test_the_manifest_lists_at_least_one_vendored_script(self):
        self.assertTrue(self.scripts)

    def test_every_vendored_script_matches_the_hash_it_was_generated_with(self):
        for name, entry in self.scripts.items():
            with self.subTest(script=name):
                body = normalized(REPO / entry["path"])
                digest = "sha256:" + hashlib.sha256(body.encode("utf-8")).hexdigest()
                self.assertEqual(
                    entry["sha256"],
                    digest,
                    f"{name} was edited in place. It is generated from {entry['source']} - change it "
                    "there and re-run that repo's vendor-hooks.ps1, or the fix is lost on the next sync.",
                )

    def test_every_vendored_script_lands_at_the_path_it_holds_upstream(self):
        # The tier's whole purpose is that a standard can name the path as a constant, which only holds
        # if source and target agree. A copy landing anywhere else silently breaks every doc naming it.
        for name, entry in self.scripts.items():
            with self.subTest(script=name):
                self.assertEqual(entry["path"], f"scripts/{name}")
                self.assertTrue((REPO / entry["path"]).is_file())

    def test_every_vendored_script_records_where_it_came_from(self):
        for name, entry in self.scripts.items():
            with self.subTest(script=name):
                self.assertTrue(entry["source"])
                self.assertRegex(entry["commit"], r"^[0-9a-f]{40}$")
                self.assertNotIn("delivery", entry)

    def test_a_vendored_script_is_wired_in_no_harness(self):
        for name in self.scripts:
            for wiring in WIRING:
                with self.subTest(script=name, wiring=wiring.name):
                    self.assertNotIn(name, wiring.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
