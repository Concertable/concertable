import sys
import tempfile
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from plan_handoff_stop import transcript_ledgers


class PlanHandoffOwnershipTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.root = Path(self.temp.name).resolve()
        self.ledger = self.root / "plans" / "EXAMPLE_PROGRESS.md"
        self.ledger.parent.mkdir()
        self.ledger.write_text("## Next Steps\n\nOpen the PR.\n", encoding="utf-8")

    def tearDown(self):
        self.temp.cleanup()

    def test_read_only_codex_tool_does_not_claim_ledger(self):
        records = [{
            "type": "response_item",
            "payload": {
                "type": "custom_tool_call",
                "name": "exec",
                "input": f'Get-Content "{self.ledger}"',
            },
        }]
        self.assertEqual(set(), transcript_ledgers(records, self.root))

    def test_read_only_claude_tool_does_not_claim_ledger(self):
        records = [{
            "type": "assistant",
            "message": {
                "role": "assistant",
                "content": [{
                    "type": "tool_use",
                    "name": "Read",
                    "input": {"file_path": str(self.ledger)},
                }],
            },
        }]
        self.assertEqual(set(), transcript_ledgers(records, self.root))

    def test_user_reference_list_claims_only_current_worktree_owner(self):
        self.ledger.write_text(
            f"- Worktree: `{self.root}`\n\n## Next Steps\n\nOpen the PR.\n",
            encoding="utf-8",
        )
        external_root = (self.root / "active-external-worktree").resolve()
        external_root.mkdir()
        external = self.root / "plans" / "EXTERNAL_PROGRESS.md"
        external.write_text(
            f"- Worktree: `{external_root}`\n\n## Next Steps\n\nContinue external work.\n",
            encoding="utf-8",
        )
        records = [{
            "type": "response_item",
            "payload": {
                "type": "message",
                "role": "user",
                "content": f"Read {self.ledger} and {external} before planning.",
            },
        }]

        self.assertEqual({self.ledger.resolve()}, transcript_ledgers(records, self.root))

    def test_cross_worktree_ledger_mutation_does_not_claim_active_owner(self):
        external_root = (self.root / "active-external-worktree").resolve()
        external_root.mkdir()
        external = self.root / "plans" / "EXTERNAL_PROGRESS.md"
        external.write_text(
            f"- Worktree: `{external_root}`\n\n## Next Steps\n\nContinue external work.\n",
            encoding="utf-8",
        )
        records = [{
            "type": "response_item",
            "payload": {
                "type": "custom_tool_call",
                "name": "exec",
                "input": (
                    'const patch = "*** Begin Patch\\n*** Update File: '
                    + str(external)
                    + '\\n*** End Patch"; await tools.apply_patch(patch);'
                ),
            },
        }]

        self.assertEqual(set(), transcript_ledgers(records, self.root))

    def test_read_only_reference_does_not_claim_missing_external_owner(self):
        missing_root = (self.root / "removed-external-worktree").resolve()
        external = self.root / "plans" / "EXTERNAL_PROGRESS.md"
        external.write_text(
            f"- Worktree: `{missing_root}`\n\n## Next Steps\n\nContinue external work.\n",
            encoding="utf-8",
        )
        records = [{
            "type": "response_item",
            "payload": {
                "type": "message",
                "role": "user",
                "content": f"Read {external} as historical context.",
            },
        }]

        self.assertEqual(set(), transcript_ledgers(records, self.root))

    def test_explicit_external_workdir_claims_that_owner(self):
        external_root = (self.root / "active-external-worktree").resolve()
        external_root.mkdir()
        external = external_root / "plans" / "EXTERNAL_PROGRESS.md"
        external.parent.mkdir()
        external.write_text(
            f"- Worktree: `{external_root}`\n\n## Next Steps\n\nContinue external work.\n",
            encoding="utf-8",
        )
        records = [{
            "type": "response_item",
            "payload": {
                "type": "custom_tool_call",
                "name": "write_file",
                "input": {
                    "path": str(external),
                    "workdir": str(external_root),
                },
            },
        }]

        self.assertEqual({external.resolve()}, transcript_ledgers(records, self.root))


if __name__ == "__main__":
    unittest.main()
