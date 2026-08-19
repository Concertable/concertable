import json
import subprocess
import sys
import tempfile
import unittest
import uuid
from pathlib import Path


HOOK = Path(__file__).resolve().parents[1] / "skill_router.py"
ROUTES = Path(__file__).resolve().parents[2] / "skill-routes.json"


class SkillRouterTests(unittest.TestCase):
    """This repo's own route table, run through the vendored router.

    The mechanism is asserted upstream in `Concertable/agent-standards` over a fixture table, so
    a route added here cannot change what the mechanism is proven to do. Duplicating a mechanism
    test here drifts: the copy asserting a malformed table fails open outlived the upstream fix
    that made it block, and only surfaced when the stale vendored router was finally refreshed.
    """

    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.root = Path(self.temp.name).resolve()
        (self.root / ".git").mkdir()
        (self.root / ".agents").mkdir()
        (self.root / ".agents" / "skill-routes.json").write_text(
            ROUTES.read_text(encoding="utf-8"), encoding="utf-8"
        )
        self.session = str(uuid.uuid4())

    def tearDown(self):
        self.temp.cleanup()

    def run_hook(self, tool="Write", path="x.cs", content="", session=None, root=None):
        payload = {
            "tool_name": tool,
            "session_id": session or self.session,
            "cwd": str(root or self.root),
            "tool_input": {"file_path": str((root or self.root) / path), "content": content},
        }
        return subprocess.run(
            [sys.executable, str(HOOK)],
            input=json.dumps(payload),
            capture_output=True,
            text=True,
        )

    def run_codex_patch(self, body, session=None):
        """Codex's shape: `apply_patch`, no file_path, every path named inside the patch body."""
        payload = {
            "tool_name": "apply_patch",
            "session_id": session or self.session,
            "cwd": str(self.root),
            "tool_input": {"input": body},
        }
        return subprocess.run(
            [sys.executable, str(HOOK)],
            input=json.dumps(payload),
            capture_output=True,
            text=True,
        )

    def test_this_repos_table_enforces_against_a_codex_patch_too(self):
        # Both harnesses run the same vendored router, so both must reach the same verdict on this
        # repo's own table - not just the one whose payload shape the router was written against.
        result = self.run_codex_patch(
            "*** Begin Patch\n"
            "*** Add File: api/Concertable.Svc/tests/Concertable.Svc.UnitTests/HostTests.cs\n"
            "+var f = new WebApplicationFactory<Program>();\n"
            "*** End Patch\n"
        )

        self.assertEqual(2, result.returncode)
        self.assertIn("integration test", result.stderr)

    def test_the_incident_fingerprint_is_blocked(self):
        result = self.run_hook(
            path="api/Concertable.ServiceDefaults/tests/Concertable.ServiceDefaults.UnitTests/RateLimitTests.cs",
            content="using Microsoft.AspNetCore.TestHost;\nvar b = WebApplication.CreateBuilder();",
        )

        self.assertEqual(2, result.returncode)
        self.assertIn("rule violation", result.stderr)

    def test_a_test_csproj_routes_to_both_testing_skills(self):
        result = self.run_hook(
            path="api/Svc/tests/Svc.Tests/Svc.Tests.csproj",
            content="<Project><PropertyGroup><IsTestProject>true</IsTestProject></PropertyGroup></Project>",
        )

        self.assertEqual(2, result.returncode)
        self.assertIn("unit-testing", result.stderr)
        self.assertIn("integration-testing", result.stderr)

    def test_a_non_test_csproj_is_allowed(self):
        result = self.run_hook(
            path="api/Svc/Svc.csproj",
            content="<Project><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>",
        )

        self.assertEqual(0, result.returncode)

    def test_the_review_query_resolves_this_repo_table(self):
        # What `review` Step 2 runs. It reads the file on disk, so the path must really exist.
        target = self.root / "api/Svc.UnitTests/A.cs"
        target.parent.mkdir(parents=True)
        target.write_text("class A { }", encoding="utf-8")

        result = subprocess.run(
            [sys.executable, str(HOOK), "--skills-for", "api/Svc.UnitTests/A.cs"],
            capture_output=True,
            text=True,
            cwd=str(self.root),
        )

        self.assertEqual(0, result.returncode)
        self.assertIn("unit-testing", result.stdout)

    def test_every_route_declares_a_path_and_at_least_one_skill(self):
        routes = json.loads(ROUTES.read_text(encoding="utf-8"))["routes"]

        self.assertTrue(routes)
        for route in routes:
            self.assertTrue(route.get("path"), route)
            self.assertTrue(route.get("skills"), route)


if __name__ == "__main__":
    unittest.main()
