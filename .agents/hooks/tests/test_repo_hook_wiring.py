import json
import unittest
from pathlib import Path


REPO = Path(__file__).resolve().parents[3]


class PluginOwnedHookWiringTests(unittest.TestCase):
    def test_codex_has_no_repo_local_hook_manifest(self):
        self.assertFalse((REPO / ".codex" / "hooks.json").exists())

    def test_claude_has_no_repo_local_hook_registration(self):
        settings = json.loads((REPO / ".claude" / "settings.json").read_text(encoding="utf-8"))
        self.assertNotIn("hooks", settings)

    def test_claude_enables_the_central_concertable_plugin(self):
        settings = json.loads((REPO / ".claude" / "settings.json").read_text(encoding="utf-8"))
        self.assertTrue(settings["enabledPlugins"]["concertable@agent-standards"])


if __name__ == "__main__":
    unittest.main()
