import json
import unittest
from pathlib import Path


REPO = Path(__file__).resolve().parents[3]


class PluginOwnedHooksTests(unittest.TestCase):
    def test_automatic_hooks_are_not_vendored_or_locally_registered(self):
        manifest = json.loads((REPO / ".agents" / "hooks" / "vendored.json").read_text(encoding="utf-8"))
        self.assertNotIn("hooks", manifest)
        self.assertFalse((REPO / ".codex" / "hooks.json").exists())
        settings = json.loads((REPO / ".claude" / "settings.json").read_text(encoding="utf-8"))
        registered = [
            hook.get("command", "")
            for event in settings.get("hooks", {}).values()
            for entry in event
            for hook in entry.get("hooks", [])
        ]
        # The provisioning bootstrap is the one hook the plugin cannot own: it is what proves the
        # plugin resolved at all. Everything else must still arrive from the plugin.
        for command in registered:
            self.assertIn("standards_provisioning.py", command)

    def test_vendored_utilities_remain_provenanced(self):
        manifest = json.loads((REPO / ".agents" / "hooks" / "vendored.json").read_text(encoding="utf-8"))
        self.assertTrue(manifest["scripts"])
        for entry in manifest["scripts"].values():
            self.assertEqual("Concertable/agent-standards", entry["source"])
            self.assertRegex(entry["commit"], r"^[0-9a-f]{40}$")


if __name__ == "__main__":
    unittest.main()
