import importlib.util
import json
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path

_HOOK_PATH = (
    Path(__file__).resolve().parents[3] / ".claude" / "hooks" / "merge-review-gate.py"
)
_spec = importlib.util.spec_from_file_location("merge_review_gate", _HOOK_PATH)
gate = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(gate)


class MergeTargetDirTests(unittest.TestCase):
    def test_double_quoted_cd_before_merge(self):
        cmd = 'cd "/repos/my worktree" && gh pr merge 1 --merge --auto'
        self.assertEqual("/repos/my worktree", gate.merge_target_dir(cmd, {}))

    def test_single_quoted_cd_before_merge(self):
        cmd = "cd '/repos/wt' && gh pr merge 1 --auto"
        self.assertEqual("/repos/wt", gate.merge_target_dir(cmd, {}))

    def test_bare_word_cd_before_merge(self):
        cmd = "cd /repos/wt && gh pr merge 1 --auto"
        self.assertEqual("/repos/wt", gate.merge_target_dir(cmd, {}))

    def test_last_cd_before_merge_wins(self):
        cmd = 'cd /a && cd "/b" && gh pr merge 1 --auto'
        self.assertEqual("/b", gate.merge_target_dir(cmd, {}))

    def test_cd_after_merge_is_ignored(self):
        cmd = "gh pr merge 1 --auto && cd /elsewhere"
        self.assertEqual("/main", gate.merge_target_dir(cmd, {"cwd": "/main"}))

    def test_payload_cwd_when_no_cd(self):
        cmd = "gh pr merge 1 --auto"
        self.assertEqual("/main", gate.merge_target_dir(cmd, {"cwd": "/main"}))

    def test_process_cwd_when_nothing(self):
        self.assertEqual(".", gate.merge_target_dir("gh pr merge 1 --auto", {}))

    def test_unbalanced_quote_not_mangled(self):
        cmd = 'cd "unbalanced && gh pr merge 1 --auto'
        self.assertEqual('"unbalanced', gate.merge_target_dir(cmd, {}))


class EndToEndTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.repo = Path(self.temp.name).resolve()
        run = lambda *a: subprocess.run(
            ["git", "-C", str(self.repo), *a], capture_output=True, text=True, check=True
        )
        run("init", "-q")
        run("config", "user.email", "t@t")
        run("config", "user.name", "t")
        run("checkout", "-q", "-b", "Feature/xyz")
        run("commit", "-q", "--allow-empty", "-m", "init")
        self.head = run("rev-parse", "HEAD").stdout.strip()
        self.reviews = self.repo / "reviews"
        self.reviews.mkdir()

    def tearDown(self):
        self.temp.cleanup()

    def _write_review(self, sha):
        (self.reviews / "Feature-xyz.md").write_text(
            "**Reviewed up to commit:** `" + sha + "`\n\n- [x] done\n", encoding="utf-8"
        )

    def _invoke(self, command, cwd=None):
        payload = {"tool_name": "Bash", "tool_input": {"command": command}}
        if cwd is not None:
            payload["cwd"] = cwd
        return subprocess.run(
            [sys.executable, str(_HOOK_PATH)],
            input=json.dumps(payload),
            capture_output=True,
            text=True,
        )

    def test_cd_to_worktree_uses_that_branch_review(self):
        self._write_review(self.head)
        # payload cwd is a DIFFERENT dir with no review — the cd target must win.
        proc = self._invoke(
            'cd "' + str(self.repo) + '" && gh pr merge 1 --merge --auto',
            cwd=str(Path(self.temp.name).parent),
        )
        self.assertEqual(0, proc.returncode, proc.stderr)

    def test_stale_review_in_target_blocks(self):
        self._write_review("0" * 40)
        proc = self._invoke('cd "' + str(self.repo) + '" && gh pr merge 1 --merge --auto')
        self.assertEqual(2, proc.returncode)
        self.assertIn("STALE", proc.stderr)

    def test_missing_review_in_target_blocks(self):
        proc = self._invoke('cd "' + str(self.repo) + '" && gh pr merge 1 --merge --auto')
        self.assertEqual(2, proc.returncode)
        self.assertIn("no review file", proc.stderr)

    def test_broken_target_dir_fails_closed(self):
        proc = self._invoke('cd "' + str(self.repo / "nope") + '" && gh pr merge 1 --auto')
        self.assertEqual(2, proc.returncode)
        self.assertIn("cannot resolve git state", proc.stderr)

    def test_non_merge_command_allowed(self):
        proc = self._invoke('cd "' + str(self.repo) + '" && gh pr view 1')
        self.assertEqual(0, proc.returncode)


if __name__ == "__main__":
    unittest.main()
