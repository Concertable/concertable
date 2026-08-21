import json
import os
import shutil
import subprocess
import unittest
import uuid
from pathlib import Path


REPO = Path(__file__).resolve().parents[3]
SCRIPTS = {
    "skill_router.py",
    "merge_review_gate.py",
    "plan_handoff_stop_launcher.py",
}


def handlers(path):
    manifest = json.loads(path.read_text(encoding="utf-8"))
    for groups in manifest["hooks"].values():
        for group in groups:
            yield from group["hooks"]


def payload(script):
    data = {
        "session_id": f"repo-hook-{uuid.uuid4().hex}",
        "cwd": str(REPO),
    }
    if script == "skill_router.py":
        data.update(
            {
                "hook_event_name": "PreToolUse",
                "tool_use_id": uuid.uuid4().hex,
                "tool_name": "Write",
                "tool_input": {},
            }
        )
    elif script == "merge_review_gate.py":
        data.update(
            {
                "hook_event_name": "PreToolUse",
                "tool_use_id": uuid.uuid4().hex,
                "tool_name": "Bash",
                "tool_input": {"command": "git status --short"},
            }
        )
    else:
        data.update(
            {
                "hook_event_name": "Stop",
                "stop_hook_active": True,
                "last_assistant_message": "Hook launcher verification.",
            }
        )
    return json.dumps(data)


class RepoHookWiringTests(unittest.TestCase):
    def assert_launched(self, result):
        self.assertEqual(0, result.returncode, result.stderr)
        combined = result.stdout + result.stderr
        self.assertNotIn("can't open file", combined)
        self.assertNotIn("is not recognized", combined)
        self.assertNotIn("HANDOFF GATE ERROR", combined)

    def test_codex_windows_commands_launch_every_repo_hook(self):
        if os.name != "nt":
            self.skipTest("Windows command contract")
        actual = list(handlers(REPO / ".codex" / "hooks.json"))
        scripts = {item["commandWindows"].split()[-1] for item in actual}
        self.assertEqual(SCRIPTS, scripts)
        for item in actual:
            script = item["commandWindows"].split()[-1]
            result = subprocess.run(
                f'cmd.exe /D /S /C "{item["commandWindows"]}"',
                input=payload(script),
                capture_output=True,
                text=True,
                cwd=REPO,
            )
            self.assert_launched(result)

    def test_claude_bash_commands_launch_every_repo_hook(self):
        git = shutil.which("git")
        self.assertIsNotNone(git)
        if os.name == "nt":
            bash = Path(git).parent.parent / "bin" / "bash.exe"
        else:
            bash = Path(shutil.which("bash"))
        self.assertTrue(bash.is_file())
        environment = os.environ.copy()
        environment["CLAUDE_PROJECT_DIR"] = str(REPO)
        actual = list(handlers(REPO / ".claude" / "settings.json"))
        scripts = {item["command"].split()[-1] for item in actual}
        self.assertEqual(SCRIPTS, scripts)
        for item in actual:
            script = item["command"].split()[-1]
            result = subprocess.run(
                [str(bash), "-c", item["command"]],
                input=payload(script),
                capture_output=True,
                text=True,
                cwd=REPO,
                env=environment,
            )
            self.assert_launched(result)


if __name__ == "__main__":
    unittest.main()
