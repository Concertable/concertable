import io
import subprocess
import sys
import tempfile
import unittest
from contextlib import redirect_stdout
from pathlib import Path
from unittest.mock import Mock, patch

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from plan_handoff_stop_launcher import TRUSTED_FILES, blob_oid, implementation_is_current, main


class PlanHandoffStopLauncherTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.root = Path(self.temp.name)

    def tearDown(self):
        self.temp.cleanup()

    @patch("plan_handoff_stop_launcher.subprocess.run")
    def test_reads_checked_out_blob_oid(self, run):
        run.return_value = Mock(returncode=0, stdout="abc123\n")

        self.assertEqual("abc123", blob_oid(self.root))
        run.assert_called_once_with(
            [
                "git",
                "-C",
                str(self.root),
                "hash-object",
                str(self.root / ".agents/hooks/plan_handoff_stop.py"),
            ],
            capture_output=True,
            text=True,
            timeout=5,
        )

    @patch("plan_handoff_stop_launcher.subprocess.run")
    def test_reads_origin_main_blob_oid_without_executing_it(self, run):
        run.return_value = Mock(returncode=0, stdout="def456\n")

        self.assertEqual("def456", blob_oid(self.root, revision="origin/main"))
        run.assert_called_once_with(
            [
                "git",
                "-C",
                str(self.root),
                "rev-parse",
                "origin/main:.agents/hooks/plan_handoff_stop.py",
            ],
            capture_output=True,
            text=True,
            timeout=5,
        )

    @patch("plan_handoff_stop_launcher.blob_oid", side_effect=["same", "same"] * len(TRUSTED_FILES))
    def test_accepts_matching_implementation(self, _):
        self.assertTrue(implementation_is_current(self.root))

    @patch("plan_handoff_stop_launcher.blob_oid", side_effect=["old", "new"])
    def test_rejects_stale_implementation(self, _):
        self.assertFalse(implementation_is_current(self.root))

    @patch("plan_handoff_stop_launcher.subprocess.run", side_effect=subprocess.TimeoutExpired("git", 5))
    def test_rejects_unverifiable_implementation(self, _):
        self.assertIsNone(blob_oid(self.root))

    @patch("plan_handoff_stop_launcher.runpy.run_path")
    @patch("plan_handoff_stop_launcher.implementation_is_current", return_value=False)
    def test_stale_launcher_warns_without_running_old_implementation(self, _, run_path):
        output = io.StringIO()
        with redirect_stdout(output):
            main()

        self.assertNotIn('"decision": "block"', output.getvalue())
        self.assertIn("systemMessage", output.getvalue())
        self.assertIn("differs from origin/main", output.getvalue())
        run_path.assert_not_called()

    @patch("plan_handoff_stop_launcher.runpy.run_path")
    @patch("plan_handoff_stop_launcher.implementation_is_current", return_value=True)
    def test_current_launcher_executes_only_checked_out_implementation(self, _, run_path):
        main()

        root = Path(__file__).resolve().parents[3]
        run_path.assert_called_once_with(
            str(root / ".agents/hooks/plan_handoff_stop.py"),
            run_name="__main__",
        )


if __name__ == "__main__":
    unittest.main()
