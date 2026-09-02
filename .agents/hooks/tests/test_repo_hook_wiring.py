import json
import unittest
from pathlib import Path


REPO = Path(__file__).resolve().parents[3]

BOOTSTRAP_HOOK = "standards_provisioning.py"


def _claude_settings():
    return json.loads((REPO / ".claude" / "settings.json").read_text(encoding="utf-8"))


def _session_start_commands(settings):
    commands = []
    for entry in settings.get("hooks", {}).get("SessionStart", []):
        for hook in entry.get("hooks", []):
            command = hook.get("command")
            if command:
                commands.append(command)
    return commands


class PluginOwnedHookWiringTests(unittest.TestCase):
    def test_codex_has_no_repo_local_hook_manifest(self):
        self.assertFalse((REPO / ".codex" / "hooks.json").exists())

    def test_claude_registers_no_repo_local_hook_the_plugin_owns(self):
        hooks = _claude_settings().get("hooks", {})
        self.assertEqual(set(hooks), {"SessionStart"}, "only the provisioning bootstrap may be repo-local")
        for command in _session_start_commands(_claude_settings()):
            self.assertIn(BOOTSTRAP_HOOK, command)

    def test_claude_enables_the_central_concertable_plugin(self):
        self.assertTrue(_claude_settings()["enabledPlugins"]["concertable@agent-standards"])


class ProvisioningBootstrapTests(unittest.TestCase):
    """The one hook that cannot live in the plugin, because it is what proves the plugin loaded.

    Deleting this wiring is what let a plugin rename strand the entire skill catalogue in silence.
    """

    def test_bootstrap_hook_script_exists(self):
        self.assertTrue((REPO / ".agents" / "hooks" / BOOTSTRAP_HOOK).is_file())

    def test_bootstrap_hook_runs_at_session_start(self):
        commands = _session_start_commands(_claude_settings())
        self.assertTrue(
            any(BOOTSTRAP_HOOK in command for command in commands),
            f"{BOOTSTRAP_HOOK} must be wired as a repo-local SessionStart hook",
        )

    def test_bootstrap_hook_is_launched_through_the_portable_runner(self):
        for command in _session_start_commands(_claude_settings()):
            if BOOTSTRAP_HOOK in command:
                self.assertIn("run-repo-hook.sh", command)
                self.assertIn("CLAUDE_PROJECT_DIR", command)

    def test_every_enabled_plugin_is_checkable(self):
        enabled = _claude_settings()["enabledPlugins"]
        for plugin_id, on in enabled.items():
            if on:
                self.assertIn("@", plugin_id, f"{plugin_id} must be name@marketplace to be verifiable")


if __name__ == "__main__":
    unittest.main()
