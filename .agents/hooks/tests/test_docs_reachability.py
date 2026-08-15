import sys
import tempfile
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from docs_reachability import repository_report


class DocsReachabilityTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.root = Path(self.temp.name).resolve()

    def tearDown(self):
        self.temp.cleanup()

    def write(self, relative, content):
        path = self.root / relative
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(content, encoding="utf-8")
        return path

    def test_clean_chain_has_no_errors(self):
        self.write("CLAUDE.md", "@AGENTS.md\n")
        self.write("AGENTS.md", "Conventions: @./agents/CODE_CONVENTIONS.md\n")
        self.write("agents/CODE_CONVENTIONS.md", "# Conventions\n")

        report = repository_report(self.root)

        self.assertEqual([], report["errors"])

    def test_plain_link_reference_counts_as_reachable(self):
        self.write("CLAUDE.md", "@AGENTS.md\n")
        self.write("AGENTS.md", "See [rules](./agents/RULES.md).\n")
        self.write("agents/RULES.md", "# Rules\n")

        report = repository_report(self.root)

        self.assertEqual([], report["errors"])

    def test_skill_reference_counts_as_reachable(self):
        self.write("CLAUDE.md", "@AGENTS.md\n")
        self.write("AGENTS.md", "# Root\n")
        self.write(".agents/skills/thing/SKILL.md", "See [rules](../../../agents/RULES.md).\n")
        self.write("agents/RULES.md", "# Rules\n")

        report = repository_report(self.root)

        self.assertEqual([], report["errors"])

    def test_agents_md_without_claude_sibling_is_an_error(self):
        self.write("CLAUDE.md", "@AGENTS.md\n")
        self.write("AGENTS.md", "# Root\n")
        self.write("app/AGENTS.md", "@./agents/CODE_CONVENTIONS.md\n")
        self.write("app/agents/CODE_CONVENTIONS.md", "# Conventions\n")

        report = repository_report(self.root)

        self.assertTrue(any("app/AGENTS.md" in error and "sibling CLAUDE.md" in error for error in report["errors"]))

    def test_claude_md_with_wrong_body_is_an_error(self):
        self.write("CLAUDE.md", "@AGENTS.md\n")
        self.write("AGENTS.md", "# Root\n")
        self.write("app/AGENTS.md", "# App\n")
        self.write("app/CLAUDE.md", "Some other content\n")

        report = repository_report(self.root)

        self.assertTrue(any("app/CLAUDE.md" in error and "must contain exactly" in error for error in report["errors"]))

    def test_unreferenced_agents_doc_is_an_orphan(self):
        self.write("CLAUDE.md", "@AGENTS.md\n")
        self.write("AGENTS.md", "# Root, no mention of the doc below\n")
        self.write("agents/ORPHAN.md", "# Nobody points at me\n")

        report = repository_report(self.root)

        self.assertTrue(any("agents/ORPHAN.md" in error and "not reachable" in error for error in report["errors"]))

    def test_reachability_follows_links_transitively_through_a_loaded_doc(self):
        self.write("CLAUDE.md", "@AGENTS.md\n")
        self.write("AGENTS.md", "@./agents/CODE_PATTERNS.md\n")
        self.write("agents/CODE_PATTERNS.md", "See [companion](./MICROSERVICE_COMMUNICATION.md).\n")
        self.write("agents/MICROSERVICE_COMMUNICATION.md", "# Communication\n")

        report = repository_report(self.root)

        self.assertEqual([], report["errors"])

    def test_dot_agents_directory_is_excluded_from_the_orphan_scan(self):
        self.write("CLAUDE.md", "@AGENTS.md\n")
        self.write("AGENTS.md", "# Root\n")
        self.write(".agents/hooks/some_module.md", "# Not a doc-chain target\n")

        report = repository_report(self.root)

        self.assertEqual([], report["errors"])


if __name__ == "__main__":
    unittest.main()
