import sys
import tempfile
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from plan_graph import ledger_errors, plan_path, repository_report


class PlanGraphTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.root = Path(self.temp.name).resolve()
        self.epic = self.root / "plans" / "epic"
        self.epic.mkdir(parents=True)
        self.plan = self.epic / "SHARED_PLAN.md"
        self.plan.write_text("# Plan\n", encoding="utf-8")
        self.roadmap = self.epic / "EPIC_ROADMAP.md"
        self.roadmap.write_text(
            "# Roadmap\n\n- [ ] **Build the graph** `epic/graph`\n",
            encoding="utf-8",
        )

    def tearDown(self):
        self.temp.cleanup()

    def write_ledger(self, name="WORKTREE_A_PROGRESS.md", next_steps="Open the PR.", extra=""):
        ledger = self.epic / name
        ledger.write_text(
            "\n".join(
                [
                    "# Progress",
                    "",
                    "- Plan: `plans/epic/SHARED_PLAN.md`",
                    "- Roadmap: `plans/epic/EPIC_ROADMAP.md`",
                    "- Roadmap item: `epic/graph`",
                    f"- Worktree: `{self.root}`",
                    "- Branch: `Feature/epic_graph`",
                    "",
                    "## Next Steps",
                    "",
                    next_steps,
                    "",
                    extra,
                    "",
                ]
            ),
            encoding="utf-8",
        )
        return ledger

    def test_declared_plan_is_authoritative_for_differently_named_ledger(self):
        ledger = self.write_ledger()

        self.assertEqual(self.plan, plan_path(ledger))
        self.assertEqual([], ledger_errors(ledger))

    def test_roadmap_item_must_have_a_stable_checklist_marker(self):
        ledger = self.write_ledger()
        self.roadmap.write_text("Roadmap prose mentions `epic/graph`.\n", encoding="utf-8")

        errors = ledger_errors(ledger)

        self.assertEqual(1, len(errors))
        self.assertIn("roadmap item marker", errors[0])

    def test_roadmap_item_key_must_match_the_epic(self):
        ledger = self.write_ledger()
        ledger.write_text(
            ledger.read_text(encoding="utf-8").replace("epic/graph", "other/graph"),
            encoding="utf-8",
        )

        errors = ledger_errors(ledger)

        self.assertEqual(1, len(errors))
        self.assertIn("must match `epic/<slug>`", errors[0])

    def test_missing_reciprocal_owner_handoff_is_rejected(self):
        owner = self.write_ledger("OWNER_PROGRESS.md")
        waiting = self.write_ledger(
            "WAITING_PROGRESS.md",
            "Blocked: The owner has not published.\n"
            "Blocked by: plans/epic/OWNER_PROGRESS.md.\n"
            "Unblock action: Publish the owner package.\n"
            "Resume when: The package restores from the feed.",
        )

        errors = ledger_errors(waiting, live_owners=False)

        self.assertTrue(owner.is_file())
        self.assertEqual(1, len(errors))
        self.assertIn("must list plans/epic/WAITING_PROGRESS.md", errors[0])

    def test_reciprocal_owner_handoff_registers_the_wait(self):
        self.write_ledger(
            "OWNER_PROGRESS.md",
            extra="## Downstream handoffs\n\n- `plans/epic/WAITING_PROGRESS.md`",
        )
        waiting = self.write_ledger(
            "WAITING_PROGRESS.md",
            "Blocked: The owner has not published.\n"
            "Blocked by: plans/epic/OWNER_PROGRESS.md.\n"
            "Unblock action: Publish the owner package.\n"
            "Resume when: The package restores from the feed.",
        )

        self.assertEqual([], ledger_errors(waiting, live_owners=False))

    def test_one_roadmap_item_cannot_have_multiple_plan_owners(self):
        self.write_ledger()
        second_plan = self.epic / "SECOND_PLAN.md"
        second_plan.write_text("# Second\n", encoding="utf-8")
        second = self.write_ledger("SECOND_PROGRESS.md")
        second.write_text(
            second.read_text(encoding="utf-8").replace("SHARED_PLAN.md", "SECOND_PLAN.md"),
            encoding="utf-8",
        )

        report = repository_report(self.root)

        self.assertEqual(1, len(report["errors"]))
        self.assertIn("multiple plan owners", report["errors"][0])

    def test_non_plan_owner_does_not_require_reciprocal_ledger(self):
        ledger = self.write_ledger(
            next_steps=(
                "Blocked: Production credentials are unavailable.\n"
                "Blocked by: Tommy.\n"
                "Unblock action: Tommy supplies the production credentials.\n"
                "Resume when: The credentials authenticate successfully."
            )
        )

        self.assertEqual([], ledger_errors(ledger))

    def test_terminal_owner_cannot_retain_pending_handoff(self):
        ledger = self.write_ledger(
            next_steps="Complete.",
            extra="## Downstream handoffs\n\n- `plans/epic/WAITING_PROGRESS.md`",
        )

        errors = ledger_errors(ledger)

        self.assertEqual(1, len(errors))
        self.assertIn("terminal ledger still has undispatched", errors[0])

    def test_legacy_graph_metadata_is_rejected(self):
        legacy_plan = self.epic / "LEGACY_PLAN.md"
        legacy_plan.write_text("# Legacy\n", encoding="utf-8")
        legacy = self.epic / "LEGACY_PROGRESS.md"
        legacy.write_text("## Next Steps\n\nOpen the PR.\n", encoding="utf-8")

        report = repository_report(self.root)

        self.assertEqual(2, len(report["errors"]))
        self.assertEqual([], report["warnings"])


if __name__ == "__main__":
    unittest.main()
