r"""SessionStart hook: prove the enabled standards plugins actually resolved.

Every other piece of agent wiring — the floor, the skill router, the merge gate — is delivered *by* the
standards plugin. That makes the plugin the one component nothing else can vouch for: when it fails to
resolve, the hooks that would have noticed are inside the thing that did not load, so the whole
catalogue disappears in silence and the session simply behaves as if no standards existed.

Two failures produce that silence, and neither is hypothetical:

* the marketplace clone is stale, so a plugin renamed upstream (``agent-process`` -> ``workflow`` ->
  ``concertable``) is no longer advertised under the name ``enabledPlugins`` asks for;
* the marketplace is current but the plugin was never installed under its new name, because naming a
  plugin in ``enabledPlugins`` enables it, it does not install it.

``autoUpdate`` on a marketplace cannot cover either one: refreshing a clone is not installing a plugin,
and a rename always strands the old install record.

So this hook is repo-local by necessity rather than by preference, and it is the only hook that may be.
It reads the on-disk plugin state directly — no network, no ``claude`` subprocess — and reports the
mismatch with the exact repair command. Anything unexpected exits 0 printing nothing: a broken
provisioning check must never wedge a session.
"""

import json
import os
import sys
from pathlib import Path

from hook_runtime import claim_invocation

for _stream in (sys.stdout, sys.stderr):
    if hasattr(_stream, "reconfigure"):
        _stream.reconfigure(encoding="utf-8", errors="replace")

ROUTES_FILE = ".agents/skill-routes.json"
SETTINGS_FILE = ".claude/settings.json"
REPAIR_COMMAND = "pwsh -File scripts/provision-agent-standards.ps1 -StandardsScope Concertable"
HOOK_NAME = "standards_provisioning"


def _read_json(path):
    try:
        return json.loads(Path(path).read_text(encoding="utf-8"))
    except (OSError, ValueError, TypeError):
        return None


def _read_payload():
    try:
        raw = sys.stdin.read()
    except Exception:
        return {}
    if not raw or not raw.strip():
        return {}
    try:
        return json.loads(raw)
    except (ValueError, TypeError):
        return {}


def _project_dir(data):
    for key in ("cwd", "workspace", "workspaceRoot", "project_dir", "projectDir"):
        value = data.get(key)
        if value:
            try:
                return Path(value)
            except (TypeError, ValueError):
                continue
    return Path.cwd()


def _repo_root(project_dir):
    try:
        current = project_dir.resolve()
    except OSError:
        return None
    for directory in (current, *current.parents):
        if (directory / ROUTES_FILE).is_file():
            return directory
    return None


def _plugins_dir():
    configured = os.environ.get("CLAUDE_CONFIG_DIR")
    base = Path(configured) if configured else Path.home() / ".claude"
    return base / "plugins"


def _advertised(marketplaces_dir, marketplace):
    manifest = _read_json(marketplaces_dir / marketplace / ".claude-plugin" / "marketplace.json")
    if not isinstance(manifest, dict):
        return None
    entries = manifest.get("plugins")
    if not isinstance(entries, list):
        return None
    return {e.get("name") for e in entries if isinstance(e, dict)}


def _installed_ids(plugins_dir):
    record = _read_json(plugins_dir / "installed_plugins.json")
    if not isinstance(record, dict):
        return None
    plugins = record.get("plugins")
    if not isinstance(plugins, dict):
        return None
    return set(plugins)


def _diagnose(enabled, plugins_dir):
    marketplaces_dir = plugins_dir / "marketplaces"
    installed = _installed_ids(plugins_dir)
    if installed is None:
        return []

    unresolved = []
    for plugin_id in enabled:
        name, separator, marketplace = plugin_id.partition("@")
        if not separator:
            continue
        advertised = _advertised(marketplaces_dir, marketplace)
        if advertised is None:
            unresolved.append((plugin_id, f"marketplace '{marketplace}' is not cloned on this machine"))
        elif name not in advertised:
            unresolved.append((plugin_id, f"'{marketplace}' does not advertise '{name}' — the clone is stale or the plugin was renamed"))
        elif plugin_id not in installed:
            unresolved.append((plugin_id, "advertised but never installed — enabling a plugin does not install it"))
    return unresolved


def main():
    data = _read_payload()
    root = _repo_root(_project_dir(data))
    if root is None:
        return 0
    if not claim_invocation(data, HOOK_NAME):
        return 0

    settings = _read_json(root / SETTINGS_FILE)
    if not isinstance(settings, dict):
        return 0
    enabled = settings.get("enabledPlugins")
    if not isinstance(enabled, dict):
        return 0
    enabled = sorted(k for k, v in enabled.items() if v is True)
    if not enabled:
        return 0

    unresolved = _diagnose(enabled, _plugins_dir())
    if not unresolved:
        return 0

    print("STANDARDS PLUGINS DID NOT RESOLVE — the skill catalogue for this repo is NOT loaded.")
    print("")
    for plugin_id, reason in unresolved:
        print(f"  {plugin_id}: {reason}")
    print("")
    print("Every standard, skill and hook this repo relies on is delivered by these plugins, so the")
    print("session is currently running without them. Tell the user to repair it and restart:")
    print("")
    print(f"  {REPAIR_COMMAND}")
    return 0


if __name__ == "__main__":
    try:
        sys.exit(main())
    except Exception:
        sys.exit(0)
