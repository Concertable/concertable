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

    def test_non_write_tool_is_ignored(self):
        self.assertEqual(0, self.run_hook(tool="Bash").returncode)

    def test_unrouted_path_is_allowed(self):
        self.assertEqual(0, self.run_hook(path="src/Whatever.cs").returncode)

    def test_first_write_to_a_routed_path_blocks_and_names_the_skill(self):
        result = self.run_hook(path="api/Svc.UnitTests/SomeTests.cs", content="public class X { }")

        self.assertEqual(2, result.returncode)
        self.assertIn("unit-testing", result.stderr)
        self.assertIn("NOT written", result.stderr)

    def test_second_write_to_the_same_route_is_allowed(self):
        self.run_hook(path="api/Svc.UnitTests/A.cs", content="class A { }")

        result = self.run_hook(path="api/Svc.UnitTests/B.cs", content="class B { }")

        self.assertEqual(0, result.returncode)

    def test_a_new_session_is_reminded_again(self):
        self.run_hook(path="api/Svc.UnitTests/A.cs", content="class A { }")

        # A fresh uuid every run: the router keys its state file by session id, and those files
        # outlive the test, so a literal id passes once and then fails for ever.
        result = self.run_hook(
            path="api/Svc.UnitTests/B.cs", content="class B { }", session=str(uuid.uuid4())
        )

        self.assertEqual(2, result.returncode)

    def test_a_deny_pattern_blocks_even_after_the_route_is_seen(self):
        self.run_hook(path="api/Svc.UnitTests/A.cs", content="class A { }")

        result = self.run_hook(
            path="api/Svc.UnitTests/B.cs",
            content="var f = new WebApplicationFactory<Program>();",
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

    def test_an_unreadable_routes_file_fails_open(self):
        (self.root / ".agents" / "skill-routes.json").write_text("{ not json", encoding="utf-8")

        result = self.run_hook(path="api/Svc.UnitTests/A.cs", content="class A { }")

        self.assertEqual(0, result.returncode)

    def test_a_repo_without_the_routes_file_is_ignored(self):
        other = Path(tempfile.mkdtemp()).resolve()
        (other / ".git").mkdir()

        result = self.run_hook(path="api/Svc.UnitTests/A.cs", content="class A { }", root=other)

        self.assertEqual(0, result.returncode)

    def test_every_route_declares_a_path_and_at_least_one_skill(self):
        routes = json.loads(ROUTES.read_text(encoding="utf-8"))["routes"]

        self.assertTrue(routes)
        for route in routes:
            self.assertTrue(route.get("path"), route)
            self.assertTrue(route.get("skills"), route)


if __name__ == "__main__":
    unittest.main()
