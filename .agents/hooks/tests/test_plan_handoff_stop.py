import json
import sys
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from plan_handoff_stop import evaluate, expected_handoff, next_steps, transcript_ledgers


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
        self.roadmap = self.root / "plans" / "launch" / "LAUNCH_ROADMAP.md"
        self.roadmap.write_text(
            "# Roadmap\n\n- [ ] **Example** `launch/example`\n",
            encoding="utf-8",
        )

    def tearDown(self):
        self.temp.cleanup()

    def write_ledger(self, next_steps, worktree=None):
        declared_worktree = worktree or self.root
        self.ledger.write_text(
            "\n".join(
                [
                    "# Progress",
                    "",
                    "- Plan: `plans/launch/EXAMPLE_PLAN.md`",
                    "- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`",
                    "- Roadmap item: `launch/example`",
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
                    "- Plan: `plans/launch/EXAMPLE_PLAN.md`",
                    "- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`",
                    "- Roadmap item: `launch/example`",
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
        roadmap = root / "plans" / "launch" / "LAUNCH_ROADMAP.md"
        roadmap.write_text(
            "# Roadmap\n\n- [ ] **Example** `launch/example`\n",
            encoding="utf-8",
        )
        ledger.write_text(
            "\n".join(
                [
                    "# Progress",
                    "",
                    "- Plan: `plans/launch/EXAMPLE_PLAN.md`",
                    "- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`",
                    "- Roadmap item: `launch/example`",
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
            "cwd": str(self.root),
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

    def handoff(self):
        return expected_handoff(
            self.ledger.resolve(),
            self.pointer(),
            next_steps(self.ledger.read_text(encoding="utf-8")),
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
        result = evaluate(self.input_with_codex_transcript(f"Ready.\n\n{self.handoff()}"))
        self.assertEqual({}, result)

    def test_allows_markdown_hard_break_whitespace_in_pointer(self):
        self.write_ledger("Run the repository code-review workflow, then open the PR.")
        handoff = self.handoff().replace("\n", "  \n")
        result = evaluate(self.input_with_codex_transcript(f"Ready.\n\n{handoff}"))
        self.assertEqual({}, result)

    def test_allows_rendered_plain_text_handoff_with_full_untruncated_reason(self):
        next_steps_body = (
            "Run incremental code review over the commits after the existing review watermark, "
            "including the domain-ownership correction. Address every clear finding, refresh "
            "current-main state, and run the read-only PR preflight. Do not push or open a PR "
            "without instruction."
        )
        self.write_ledger(next_steps_body)
        rendered_pointer = self.pointer().replace("`", "")
        message = (
            f"Why: {self.ledger.name} owns unfinished work from this turn: {next_steps_body}\n\n"
            "Only run this continuation if no agent or session is already working in "
            f"{self.root}.\n\n{rendered_pointer}"
        )
        result = evaluate(self.input_with_codex_transcript(message))
        self.assertEqual({}, result)

    def test_normalized_handoff_still_must_end_with_pointer(self):
        self.write_ledger("Run the repository code-review workflow, then open the PR.")
        rendered_handoff = self.handoff().replace("`", "").replace("text\n", "").replace("\n", " ")
        result = evaluate(
            self.input_with_codex_transcript(f"{rendered_handoff}\n\nLet me know.")
        )
        self.assertEqual("block", result["decision"])

    def test_allows_renderer_line_wrap_after_path_hyphen(self):
        declared_worktree = self.root.parent / "typed-result_auth-outcomes"
        declared_worktree.mkdir()
        self.write_ledger("Run the repository code-review workflow, then open the PR.", declared_worktree)
        rendered_handoff = (
            self.handoff()
            .replace("`", "")
            .replace("```text\n", "")
            .replace("\n```", "")
            .replace("auth-outcomes", "auth-\noutcomes")
        )
        result = evaluate(self.input_with_codex_transcript(rendered_handoff))
        self.assertEqual({}, result)

    def test_bare_pointer_does_not_pass_without_reason_and_collision_warning(self):
        self.write_ledger("Open the PR after review.")
        result = evaluate(
            self.input_with_codex_transcript(f"```text\n{self.pointer()}\n```")
        )
        self.assertEqual("block", result["decision"])
        self.assertIn("Why:", result["reason"])
        self.assertIn("Only run this continuation if no agent or session", result["reason"])

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
            "Blocked by: GitHub PR #123.\n"
            "Unblock action: The PR #123 owner must follow it to a terminal merge.\n"
            "Resume when: GitHub reports PR #123 merged."
        )
        result = evaluate(
            self.input_with_codex_transcript(
                "Blocked: PR #123 has not merged.\n"
                "Blocked by: GitHub PR #123.\n"
                "Unblock action: The PR #123 owner must follow it to a terminal merge.\n"
                "Resume when: GitHub reports PR #123 merged."
            )
        )
        self.assertEqual({}, result)

    def test_registered_downstream_wait_reports_blocker_without_pointer(self):
        self.write_ledger(
            "Blocked: Checkpoints 6-7 require the owner's platform-sync PR to merge.\n"
            "Blocked by: owner ledger.\n"
            "Unblock action: The owner ledger must follow the sync and dispatch this dependent.\n"
            "Resume when: The owner records the merged sync in this ledger.\n\n"
            "The owner ledger lists this ledger under `## Downstream handoffs`."
        )
        result = evaluate(
            self.input_with_codex_transcript(
                "Blocked: Checkpoints 6-7 require the owner's platform-sync PR to merge.\n"
                "Blocked by: owner ledger.\n"
                "Unblock action: The owner ledger must follow the sync and dispatch this dependent.\n"
                "Resume when: The owner records the merged sync in this ledger."
            )
        )
        self.assertEqual({}, result)

    def test_blocker_report_must_include_every_actionable_value(self):
        self.write_ledger(
            "Blocked: Commit abc is unavailable.\n"
            "Blocked by: upstream branch.\n"
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
            "Blocked by: upstream branch.\n"
            "Unblock action: Push a branch containing commit abc.\n"
            "Resume when: git cat-file resolves commit abc."
        )
        message = (
            "Blocked: Commit abc is unavailable.\n"
            "Blocked by: upstream branch.\n"
            "Unblock action: Push a branch containing commit abc.\n"
            f"Resume when: git cat-file resolves commit abc.\n\n{self.pointer()}"
        )
        result = evaluate(self.input_with_codex_transcript(message))
        self.assertEqual("block", result["decision"])
        self.assertIn("remove the blocked plan's continuation pointer", result["reason"])

    def test_reports_blocker_contract_and_actionable_pointer_together(self):
        blocked = self.root / "plans" / "launch" / "OWNER_PROGRESS.md"
        blocked_plan = blocked.with_name("OWNER_PLAN.md")
        active = self.root / "plans" / "launch" / "DEPENDENT_PROGRESS.md"
        active_plan = active.with_name("DEPENDENT_PLAN.md")
        blocked_plan.write_text("# Plan\n", encoding="utf-8")
        active_plan.write_text("# Plan\n", encoding="utf-8")
        blocker = (
            "Blocked: The package is not published.\n"
            "Blocked by: package owner.\n"
            "Unblock action: Publish and verify the package.\n"
            "Resume when: The production feed restores it."
        )
        blocked.write_text(
            "- Plan: `plans/launch/OWNER_PLAN.md`\n"
            "- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`\n"
            "- Roadmap item: `launch/example`\n"
            f"- Worktree: `{self.root}`\n\n## Next Steps\n\n{blocker}\n",
            encoding="utf-8",
        )
        active.write_text(
            "- Plan: `plans/launch/DEPENDENT_PLAN.md`\n"
            "- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`\n"
            "- Roadmap item: `launch/example`\n"
            f"- Worktree: `{self.root}`\n\n## Next Steps\n\nOpen the dependent PR.\n",
            encoding="utf-8",
        )
        active_pointer = (
            f"cd {self.root}\n"
            "Read @plans/launch/DEPENDENT_PLAN.md and "
            "@plans/launch/DEPENDENT_PROGRESS.md and do what its `## Next Steps` says."
        )
        transcript = self.root / "mixed-handoff-transcript.jsonl"
        records = [
            {
                "type": "response_item",
                "payload": {
                    "type": "custom_tool_call",
                    "name": "exec",
                    "input": (
                        'const patch = "*** Begin Patch\\n*** Update File: '
                        'plans/launch/OWNER_PROGRESS.md\\n*** Update File: '
                        'plans/launch/DEPENDENT_PROGRESS.md\\n*** End Patch"; '
                        f'const options = {{workdir: "{self.root}"}};'
                    ),
                },
            }
        ]
        transcript.write_text(
            "\n".join(json.dumps(record) for record in records),
            encoding="utf-8",
        )
        data = {
            "cwd": str(self.root),
            "transcript_path": str(transcript),
            "last_assistant_message": "Implementation is complete.",
        }

        result = evaluate(data)

        self.assertEqual("block", result["decision"])
        self.assertIn("Blocked: The package is not published.", result["reason"])
        self.assertIn(active_pointer, result["reason"])

        active_handoff = expected_handoff(
            active.resolve(),
            active_pointer,
            next_steps(active.read_text(encoding="utf-8")),
        )
        data["last_assistant_message"] = f"{blocker}\n\n{active_handoff}"
        self.assertEqual({}, evaluate(data))

        data["last_assistant_message"] = active_handoff
        rejection = evaluate(data)
        self.assertEqual("block", rejection["decision"])
        data["last_assistant_message"] = rejection["reason"]
        self.assertEqual({}, evaluate(data))

    def test_legacy_blocker_requires_structured_contract(self):
        self.write_ledger("Waiting for PR #123 to merge; its owner will surface this plan when ready.")
        result = evaluate(self.input_with_codex_transcript("Waiting for PR #123."))
        self.assertEqual("block", result["decision"])
        self.assertIn("`Blocked by:`", result["reason"])
        self.assertIn("`Unblock action:`", result["reason"])

    def test_old_three_line_blocker_is_rejected_without_a_pointer(self):
        self.write_ledger(
            "Blocked: Commit abc is unavailable.\n"
            "Unblock action: Push a branch containing commit abc.\n"
            "Resume when: git cat-file resolves commit abc."
        )

        result = evaluate(self.input_with_codex_transcript("Commit abc is unavailable."))

        self.assertEqual("block", result["decision"])
        self.assertIn("`Blocked by:`", result["reason"])
        self.assertNotIn(self.pointer(), result["reason"])

    def test_invalid_plan_graph_blocks_without_a_pointer(self):
        self.write_ledger("Open the PR.")
        self.roadmap.write_text("# Roadmap\n", encoding="utf-8")

        result = evaluate(self.input_with_codex_transcript("Implementation is complete."))

        self.assertEqual("block", result["decision"])
        self.assertIn("PLAN GRAPH GATE", result["reason"])
        self.assertNotIn(self.pointer(), result["reason"])

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
        typed_roadmap = typed_result / "TYPED_RESULT_ROADMAP.md"
        typed_roadmap.write_text(
            "- [ ] **Integration** `typed-result/integration`\n"
            "- [ ] **B2B** `typed-result/b2b`\n",
            encoding="utf-8",
        )
        dotnet_roadmap = unions / "DOTNET_ROADMAP.md"
        dotnet_roadmap.write_text(
            "- [ ] **Unions** `dotnet-11/unions`\n",
            encoding="utf-8",
        )
        owner.write_text(
            "- Plan: `plans/typed-result/REUNION_INTEGRATION_PLAN.md`\n"
            "- Roadmap: `plans/typed-result/TYPED_RESULT_ROADMAP.md`\n"
            "- Roadmap item: `typed-result/integration`\n\n"
            "## Next Steps\n\nCreate the integration worktree.\n\n"
            "## Downstream handoffs\n\n- `plans/typed-result/B2B_PROGRESS.md`\n",
            encoding="utf-8",
        )
        b2b_steps = (
            "Blocked: ReUnion platform sync has not merged.\n"
            "Blocked by: plans/typed-result/REUNION_INTEGRATION_PROGRESS.md.\n"
            "Unblock action: The owner at `plans/typed-result/REUNION_INTEGRATION_PROGRESS.md` "
            "must merge it.\n"
            "Resume when: Main contains the ReUnion platform pin."
        )
        b2b.write_text(
            "- Plan: `plans/typed-result/B2B_PLAN.md`\n"
            "- Roadmap: `plans/typed-result/TYPED_RESULT_ROADMAP.md`\n"
            "- Roadmap item: `typed-result/b2b`\n\n"
            f"## Next Steps\n\n{b2b_steps}\n\n"
            "## Downstream handoffs\n\n- `plans/dotnet-11/B2B_WORKFLOW_UNIONS_PROGRESS.md`\n",
            encoding="utf-8",
        )
        union_steps = (
            "Blocked: B2B delivery is not terminal.\n"
            "Blocked by: plans/typed-result/B2B_PROGRESS.md.\n"
            "Unblock action: The owner at `plans/typed-result/B2B_PROGRESS.md` must finish it.\n"
            "Resume when: Main contains the B2B work."
        )
        workflow_unions.write_text(
            "- Plan: `plans/dotnet-11/B2B_WORKFLOW_UNIONS_PLAN.md`\n"
            "- Roadmap: `plans/dotnet-11/DOTNET_ROADMAP.md`\n"
            "- Roadmap item: `dotnet-11/unions`\n\n"
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

    def test_cross_worktree_mutation_does_not_claim_foreign_ledger(self):
        foreign_root = (Path(self.temp.name) / "foreign-worktree").resolve()
        foreign_ledger = self.write_plan_pair(
            foreign_root,
            "Open the foreign PR.",
            foreign_root,
            branch="Feature/foreign",
        )
        records = [
            {
                "type": "response_item",
                "payload": {
                    "type": "custom_tool_call",
                    "name": "exec",
                    "input": (
                        f'const patch = "*** Begin Patch\\n*** Update File: {foreign_ledger}'
                        '\\n*** End Patch"; await tools.apply_patch(patch);'
                    ),
                },
            }
        ]

        self.assertEqual(set(), transcript_ledgers(records, self.root))

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
                        '\\n*** End Patch"; await tools.apply_patch(patch); '
                        f'const options = {{workdir: "{self.root}"}};'
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
        records = [
            {
                "type": "response_item",
                "payload": {
                    "type": "custom_tool_call",
                    "name": "exec",
                    "input": (
                        f'const patch = "*** Begin Patch\\n*** Update File: {path}'
                        '\\n*** End Patch"; await tools.apply_patch(patch); '
                        f'const options = {{workdir: "{path.parents[2]}"}};'
                    ),
                },
            }
            for path in (first, second)
        ]
        transcript.write_text(
            "\n".join(json.dumps(record) for record in records),
            encoding="utf-8",
        )
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
