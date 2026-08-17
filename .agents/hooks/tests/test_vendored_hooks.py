import hashlib
import json
import unittest
from pathlib import Path


HOOKS = Path(__file__).resolve().parents[1]
REPO = HOOKS.parents[1]
MANIFEST = HOOKS / "vendored.json"
WIRING = (REPO / ".claude" / "settings.json", REPO / ".codex" / "hooks.json")


def normalized(path):
    return path.read_bytes().decode("utf-8").replace("\r\n", "\n")


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
        # spent its first life in .claude/settings.json alone, so Codex never ran it.
        for name in self.entries:
            for wiring in WIRING:
                with self.subTest(hook=name, wiring=wiring.name):
                    self.assertIn(name, wiring.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
