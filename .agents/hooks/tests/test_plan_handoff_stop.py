import json
import sys
import tempfile
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from plan_handoff_stop import evaluate


class PlanHandoffStopTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.root = Path(self.temp.name) / "Concertable.worktrees" / "Feature" / "launch_example"
        self.root.mkdir(parents=True)
        self.root = self.root.resolve()
        self.ledger = self.root / "plans" / "launch" / "EXAMPLE_PROGRESS.md"
        self.plan = self.root / "plans" / "launch" / "EXAMPLE_PLAN.md"
        self.plan.parent.mkdir(parents=True)
        self.plan.write_text("# Plan\n", encoding="utf-8")

    def tearDown(self):
        self.temp.cleanup()

    def write_ledger(self, next_steps):
        self.ledger.write_text(
            "\n".join(
                [
                    "# Progress",
                    "",
                    f"- Worktree: `{self.root}`",
                    "- Branch: `Feature/launch_example`",
                    "",
                    "## Next Steps",
                    "",
                    next_steps,
                    "",
                    "## Completed work",
                    "",
                ]
            ),
            encoding="utf-8",
        )

    def input_with_codex_transcript(self, message):
        transcript = self.root / "transcript.jsonl"
        records = [
            {"type": "response_item", "payload": {"type": "message", "role": "user", "content": []}},
            {
                "type": "response_item",
                "payload": {
                    "type": "custom_tool_call",
                    "name": "exec",
                    "input": (
                        'const r = await tools.shell_command({command: "Get-Content '
                        'plans\\launch\\EXAMPLE_PROGRESS.md", workdir: "'
                        + str(self.root)
                        + '"});'
                    ),
                },
            },
        ]
        transcript.write_text("\n".join(json.dumps(record) for record in records), encoding="utf-8")
        return {
            "cwd": str(Path(self.temp.name) / "unrelated-main-checkout"),
            "transcript_path": str(transcript),
            "last_assistant_message": message,
        }

    def input_with_claude_transcript(self, message):
        transcript = self.root / "claude-transcript.jsonl"
        records = [
            {"type": "user", "message": {"role": "user", "content": "Continue the plan."}},
            {
                "type": "assistant",
                "message": {
                    "role": "assistant",
                    "content": [
                        {
                            "type": "tool_use",
                            "name": "Read",
                            "input": {"file_path": str(self.ledger)},
                        }
                    ],
                },
            },
        ]
        transcript.write_text("\n".join(json.dumps(record) for record in records), encoding="utf-8")
        return {
            "cwd": str(Path(self.temp.name) / "unrelated-main-checkout"),
            "transcript_path": str(transcript),
            "last_assistant_message": message,
        }

    def pointer(self):
        return (
            f"cd {self.root}\n"
            "Read @plans/launch/EXAMPLE_PLAN.md and @plans/launch/EXAMPLE_PROGRESS.md "
            "and do what its `## Next Steps` says."
        )

    def test_blocks_local_completion_without_pointer(self):
        self.write_ledger("Run the repository code-review workflow, then open the PR.")
        result = evaluate(self.input_with_codex_transcript("Implementation is complete and committed."))
        self.assertEqual("block", result["decision"])
        self.assertIn(self.pointer(), result["reason"])

    def test_allows_exact_pointer(self):
        self.write_ledger("Run the repository code-review workflow, then open the PR.")
        result = evaluate(self.input_with_codex_transcript(f"Ready.\n\n```text\n{self.pointer()}\n```"))
        self.assertEqual({}, result)

    def test_paraphrased_next_steps_does_not_pass(self):
        self.write_ledger("Run the repository code-review workflow, then open the PR.")
        result = evaluate(self.input_with_codex_transcript("Next steps are code review and a PR."))
        self.assertEqual("block", result["decision"])

    def test_claude_transcript_blocks_missing_pointer(self):
        self.write_ledger("Run the repository code-review workflow, then open the PR.")
        result = evaluate(self.input_with_claude_transcript("Implementation is complete."))
        self.assertEqual("block", result["decision"])

    def test_terminal_ledger_needs_no_pointer(self):
        self.write_ledger("Complete")
        result = evaluate(self.input_with_codex_transcript("Everything is complete."))
        self.assertEqual({}, result)

    def test_inflight_owner_wait_needs_no_pointer(self):
        self.write_ledger("Waiting for PR #123 to merge; its owner will surface this plan when ready.")
        result = evaluate(self.input_with_codex_transcript("Waiting for PR #123."))
        self.assertEqual({}, result)


if __name__ == "__main__":
    unittest.main()
