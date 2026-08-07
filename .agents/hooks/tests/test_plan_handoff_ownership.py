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


if __name__ == "__main__":
    unittest.main()
