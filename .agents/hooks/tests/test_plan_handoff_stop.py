import json
import sys
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from plan_handoff_stop import evaluate, transcript_ledgers


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

    def write_ledger(self, next_steps, worktree=None):
        declared_worktree = worktree or self.root
        self.ledger.write_text(
            "\n".join(
                [
                    "# Progress",
                    "",
                    f"- Worktree: `{declared_worktree}`",
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

    def write_ledger_without_next_steps(self):
        self.ledger.write_text(
            "\n".join(
                [
                    "# Progress",
                    "",
                    f"- Worktree: `{self.root}`",
                    "- Branch: `Feature/launch_example`",
                    "",
                    "## Completed work",
                    "",
                ]
            ),
            encoding="utf-8",
        )

    def write_plan_pair(self, root, next_steps, worktree, branch="Feature/launch_example"):
        ledger = root / "plans" / "launch" / "EXAMPLE_PROGRESS.md"
        plan = root / "plans" / "launch" / "EXAMPLE_PLAN.md"
        plan.parent.mkdir(parents=True, exist_ok=True)
        plan.write_text("# Plan\n", encoding="utf-8")
        ledger.write_text(
            "\n".join(
                [
                    "# Progress",
                    "",
                    f"- Worktree: `{worktree}`",
                    f"- Branch: `{branch}`",
                    "",
                    "## Next Steps",
                    "",
                    next_steps,
                    "",
                ]
            ),
            encoding="utf-8",
        )
        return ledger.resolve()

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
                        'const patch = "*** Begin Patch\\n*** Update File: '
                        'plans\\launch\\EXAMPLE_PROGRESS.md\\n*** End Patch"; '
                        'await tools.apply_patch(patch); const options = {workdir: "'
                        + str(self.root)
                        + '"};'
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
                            "name": "Edit",
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

    def input_without_ledger_reference(self, message):
        transcript = self.root / "unrelated-transcript.jsonl"
        record = {"type": "response_item", "payload": {"type": "message", "role": "user"}}
        transcript.write_text(json.dumps(record), encoding="utf-8")
        return {
            "cwd": str(self.root),
            "transcript_path": str(transcript),
            "last_assistant_message": message,
        }

    def input_with_codex_tool_output(self, message):
        transcript = self.root / "tool-output-transcript.jsonl"
        records = [
            {
                "type": "response_item",
                "payload": {"type": "message", "role": "user", "content": "Explain the result."},
            },
            {
                "type": "custom_tool_call_output",
                "payload": {
                    "type": "custom_tool_call_output",
                    "output": f"Read {self.ledger} from workdir: '{self.root}'",
                },
            },
        ]
        transcript.write_text("\n".join(json.dumps(record) for record in records), encoding="utf-8")
        return {
            "cwd": str(self.root),
            "transcript_path": str(transcript),
            "last_assistant_message": message,
        }

    def input_with_injected_hook_prompt(self, message):
        transcript = self.root / "hook-prompt-transcript.jsonl"
        records = [
            {
                "type": "response_item",
                "payload": {"type": "message", "role": "user", "content": "Why was that emitted?"},
            },
            {
                "type": "response_item",
                "payload": {
                    "type": "message",
                    "role": "user",
                    "content": (
                        '<hook_prompt hook_run_id="stop:3:hooks.json">HANDOFF GATE: '
                        f"Read @{self.ledger}</hook_prompt>"
                    ),
                },
            },
        ]
        transcript.write_text("\n".join(json.dumps(record) for record in records), encoding="utf-8")
        return {
            "cwd": str(self.root),
            "transcript_path": str(transcript),
            "last_assistant_message": message,
        }

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

    def test_pointer_followed_by_prose_does_not_pass(self):
        self.write_ledger("Run the repository code-review workflow, then open the PR.")
        result = evaluate(self.input_with_codex_transcript(f"{self.pointer()}\n\nLet me know."))
        self.assertEqual("block", result["decision"])

    def test_missing_next_steps_fails_closed(self):
        self.write_ledger_without_next_steps()
        result = evaluate(self.input_with_codex_transcript("Everything is complete."))
        self.assertEqual("block", result["decision"])
        self.assertIn("missing its required `## Next Steps`", result["reason"])

    def test_claude_transcript_blocks_missing_pointer(self):
        self.write_ledger("Run the repository code-review workflow, then open the PR.")
        result = evaluate(self.input_with_claude_transcript("Implementation is complete."))
        self.assertEqual("block", result["decision"])

    def test_terminal_ledger_needs_no_pointer(self):
        self.write_ledger("Complete")
        result = evaluate(self.input_with_codex_transcript("Everything is complete."))
        self.assertEqual({}, result)

    def test_inflight_owner_wait_reports_blocker_without_pointer(self):
        self.write_ledger(
            "Blocked: PR #123 has not merged.\n"
            "Unblock action: The PR #123 owner must follow it to a terminal merge.\n"
            "Resume when: GitHub reports PR #123 merged."
        )
        result = evaluate(
            self.input_with_codex_transcript(
                "Blocked: PR #123 has not merged.\n"
                "Unblock action: The PR #123 owner must follow it to a terminal merge.\n"
                "Resume when: GitHub reports PR #123 merged."
            )
        )
        self.assertEqual({}, result)

    def test_registered_downstream_wait_reports_blocker_without_pointer(self):
        self.write_ledger(
            "Blocked: Checkpoints 6-7 require the owner's platform-sync PR to merge.\n"
            "Unblock action: The owner ledger must follow the sync and dispatch this dependent.\n"
            "Resume when: The owner records the merged sync in this ledger.\n\n"
            "The owner ledger lists this ledger under `## Downstream handoffs`."
        )
        result = evaluate(
            self.input_with_codex_transcript(
                "Blocked: Checkpoints 6-7 require the owner's platform-sync PR to merge.\n"
                "Unblock action: The owner ledger must follow the sync and dispatch this dependent.\n"
                "Resume when: The owner records the merged sync in this ledger."
            )
        )
        self.assertEqual({}, result)

    def test_blocker_report_must_include_every_actionable_value(self):
        self.write_ledger(
            "Blocked: Commit abc is unavailable.\n"
            "Unblock action: Push a branch containing commit abc.\n"
            "Resume when: git cat-file resolves commit abc."
        )
        result = evaluate(self.input_with_codex_transcript("Commit abc is unavailable."))
        self.assertEqual("block", result["decision"])
        self.assertIn("Push a branch containing commit abc.", result["reason"])
        self.assertIn("git cat-file resolves commit abc.", result["reason"])

    def test_blocked_plan_pointer_is_rejected(self):
        self.write_ledger(
            "Blocked: Commit abc is unavailable.\n"
            "Unblock action: Push a branch containing commit abc.\n"
            "Resume when: git cat-file resolves commit abc."
        )
        message = (
            "Blocked: Commit abc is unavailable.\n"
            "Unblock action: Push a branch containing commit abc.\n"
            f"Resume when: git cat-file resolves commit abc.\n\n{self.pointer()}"
        )
        result = evaluate(self.input_with_codex_transcript(message))
        self.assertEqual("block", result["decision"])
        self.assertIn("remove the blocked plan's continuation pointer", result["reason"])

    def test_legacy_blocker_requires_structured_contract(self):
        self.write_ledger("Waiting for PR #123 to merge; its owner will surface this plan when ready.")
        result = evaluate(self.input_with_codex_transcript("Waiting for PR #123."))
        self.assertEqual("block", result["decision"])
        self.assertIn("`Unblock action:`", result["reason"])

    def test_blocked_work_without_registered_suppression_still_needs_pointer(self):
        self.write_ledger(
            "Checkpoint 6 is blocked, but update the error records and their contract tests now."
        )
        result = evaluate(self.input_with_codex_transcript("Checkpoint 6 is blocked."))
        self.assertEqual("block", result["decision"])

    def test_patch_claims_targets_without_claiming_referenced_dependency_ledgers(self):
        typed_result = self.root / "plans" / "typed-result"
        unions = self.root / "plans" / "dotnet-11"
        owner = typed_result / "REUNION_INTEGRATION_PROGRESS.md"
        b2b = typed_result / "B2B_PROGRESS.md"
        workflow_unions = unions / "B2B_WORKFLOW_UNIONS_PROGRESS.md"
        for ledger in (owner, b2b, workflow_unions):
            ledger.parent.mkdir(parents=True, exist_ok=True)
            ledger.with_name(ledger.name.replace("_PROGRESS.md", "_PLAN.md")).write_text(
                "# Plan\n", encoding="utf-8"
            )
        owner.write_text(
            "## Next Steps\n\nCreate the integration worktree.\n",
            encoding="utf-8",
        )
        b2b_steps = (
            "Blocked: ReUnion platform sync has not merged.\n"
            "Unblock action: The owner at `plans/typed-result/REUNION_INTEGRATION_PROGRESS.md` "
            "must merge it.\n"
            "Resume when: Main contains the ReUnion platform pin."
        )
        b2b.write_text(f"## Next Steps\n\n{b2b_steps}\n", encoding="utf-8")
        union_steps = (
            "Blocked: B2B delivery is not terminal.\n"
            "Unblock action: The owner at `plans/typed-result/B2B_PROGRESS.md` must finish it.\n"
            "Resume when: Main contains the B2B work."
        )
        workflow_unions.write_text(
            f"## Next Steps\n\n{union_steps}\n",
            encoding="utf-8",
        )
        transcript = self.root / "dependency-chain-transcript.jsonl"
        records = [
            {
                "type": "response_item",
                "payload": {
                    "type": "message",
                    "role": "user",
                    "content": "Create the workflow-unions plan.",
                },
            },
            {
                "type": "response_item",
                "payload": {
                    "type": "custom_tool_call",
                    "name": "exec",
                    "input": (
                        'const patch = "*** Begin Patch\\n*** Update File: '
                        'plans/typed-result/B2B_PROGRESS.md\\n'
                        '+Unblock action: The owner at '
                        '`plans/typed-result/REUNION_INTEGRATION_PROGRESS.md` must merge it.\\n'
                        '*** End Patch"; const options = {workdir: "'
                        + str(self.root)
                        + '"};'
                    ),
                },
            },
            {
                "type": "response_item",
                "payload": {
                    "type": "custom_tool_call",
                    "name": "exec",
                    "input": (
                        'const patch = "*** Begin Patch\\n*** Update File: '
                        'plans/dotnet-11/B2B_WORKFLOW_UNIONS_PROGRESS.md\\n'
                        '+Unblock action: The owner at `plans/typed-result/B2B_PROGRESS.md` '
                        'must finish it.\\n*** End Patch"; const options = {workdir: "'
                        + str(self.root)
                        + '"};'
                    ),
                },
            },
        ]
        transcript.write_text(
            "\n".join(json.dumps(record) for record in records),
            encoding="utf-8",
        )
        claimed = transcript_ledgers(records, self.root)
        self.assertEqual({b2b.resolve(), workflow_unions.resolve()}, claimed)

        result = evaluate(
            {
                "cwd": str(self.root),
                "transcript_path": str(transcript),
                "last_assistant_message": f"{union_steps}\n\n{b2b_steps}",
            }
        )
        self.assertEqual({}, result)

    def test_relative_reference_resolves_only_against_its_tool_workdir(self):
        alternate_root = (Path(self.temp.name) / "unrelated-main-checkout").resolve()
        alternate_ledger = self.write_plan_pair(
            alternate_root,
            "Open the alternate PR.",
            alternate_root,
        )
        self.write_ledger("Open the owner PR.")
        records = [
            {
                "type": "response_item",
                "payload": {
                    "type": "custom_tool_call",
                    "name": "exec",
                    "input": (
                        'const patch = "*** Begin Patch\\n*** Update File: '
                        'plans\\launch\\EXAMPLE_PROGRESS.md\\n*** End Patch"; '
                        'await tools.apply_patch(patch); const options = {workdir: "'
                        + str(self.root)
                        + '"};'
                    ),
                },
            }
        ]
        self.assertEqual(
            {self.ledger.resolve()},
            transcript_ledgers(records, alternate_root),
        )
        self.assertNotIn(alternate_ledger, transcript_ledgers(records, alternate_root))

    def test_structured_tool_workdir_resolves_relative_reference(self):
        self.write_ledger("Open the owner PR.")
        records = [
            {
                "type": "response_item",
                "payload": {
                    "type": "function_call",
                    "name": "write_file",
                    "arguments": {
                        "path": "plans/launch/EXAMPLE_PROGRESS.md",
                        "workdir": str(self.root),
                    },
                },
            }
        ]
        unrelated = Path(self.temp.name) / "unrelated-main-checkout"
        self.assertEqual({self.ledger.resolve()}, transcript_ledgers(records, unrelated))

    def test_absolute_reference_is_not_resolved_again_as_relative(self):
        alternate_root = (Path(self.temp.name) / "unrelated-main-checkout").resolve()
        self.write_plan_pair(alternate_root, "Open the alternate PR.", alternate_root)
        self.write_ledger("Open the owner PR.")
        records = [
            {
                "type": "response_item",
                "payload": {
                    "type": "custom_tool_call",
                    "name": "exec",
                    "input": (
                        f'const patch = "*** Begin Patch\\n*** Update File: {self.ledger}'
                        '\\n*** End Patch"; await tools.apply_patch(patch);'
                    ),
                },
            }
        ]
        self.assertEqual({self.ledger.resolve()}, transcript_ledgers(records, alternate_root))

    def test_owner_copy_wins_over_stale_worktree_copy(self):
        self.write_ledger("Open the owner PR.")
        stale_root = (Path(self.temp.name) / "stale-checkout").resolve()
        stale_ledger = self.write_plan_pair(
            stale_root,
            "Open the stale PR.",
            stale_root / "missing-owner",
        )
        transcript = self.root / "duplicate-transcript.jsonl"
        record = {
            "type": "response_item",
            "payload": {
                "type": "message",
                "role": "user",
                "content": [
                    {"type": "input_text", "text": str(stale_ledger)},
                    {"type": "input_text", "text": str(self.ledger)},
                ],
            },
        }
        transcript.write_text(json.dumps(record), encoding="utf-8")
        result = evaluate(
            {
                "cwd": str(self.root),
                "transcript_path": str(transcript),
                "last_assistant_message": "Implementation is complete.",
            }
        )
        self.assertEqual("block", result["decision"])
        self.assertIn(self.pointer(), result["reason"])
        self.assertNotIn("missing-owner", result["reason"])

    def test_duplicate_logical_ledgers_emit_one_pointer(self):
        missing_owner = Path(self.temp.name) / "missing-owner"
        first = self.write_plan_pair(
            (Path(self.temp.name) / "copy-one").resolve(),
            "Open the PR.",
            missing_owner,
        )
        second = self.write_plan_pair(
            (Path(self.temp.name) / "copy-two").resolve(),
            "Open the PR.",
            missing_owner,
        )
        transcript = self.root / "logical-duplicate-transcript.jsonl"
        record = {
            "type": "response_item",
            "payload": {
                "type": "message",
                "role": "user",
                "content": [
                    {"type": "input_text", "text": str(first)},
                    {"type": "input_text", "text": str(second)},
                ],
            },
        }
        transcript.write_text(json.dumps(record), encoding="utf-8")
        result = evaluate(
            {
                "cwd": str(self.root),
                "transcript_path": str(transcript),
                "last_assistant_message": "Implementation is complete.",
            }
        )
        self.assertEqual("block", result["decision"])
        self.assertEqual(1, result["reason"].count("/worktree create Feature/launch_example"))

    def test_unrelated_turn_in_plan_worktree_needs_no_pointer(self):
        self.write_ledger("Run the repository code-review workflow, then open the PR.")
        with patch("plan_handoff_stop.branch_ledgers", return_value={self.ledger}) as fallback:
            result = evaluate(self.input_without_ledger_reference("The answer is 42."))
        self.assertEqual({}, result)
        fallback.assert_not_called()

    def test_tool_output_does_not_claim_ledger_for_session(self):
        self.write_ledger("Run the repository code-review workflow, then open the PR.")
        result = evaluate(self.input_with_codex_tool_output("The output mentions another plan."))
        self.assertEqual({}, result)

    def test_injected_hook_prompt_does_not_claim_ledger_for_session(self):
        self.write_ledger("Run the repository code-review workflow, then open the PR.")
        result = evaluate(self.input_with_injected_hook_prompt("That pointer was unrelated."))
        self.assertEqual({}, result)

    def test_missing_worktree_uses_create_opener(self):
        missing = self.root.parent / "launch_not_created"
        self.write_ledger("Implement the plan.", worktree=missing)
        result = evaluate(self.input_with_codex_transcript("The plan is ready."))
        self.assertEqual("block", result["decision"])
        self.assertIn("/worktree create Feature/launch_example", result["reason"])


if __name__ == "__main__":
    unittest.main()
