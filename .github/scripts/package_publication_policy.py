"""Validate an immutable, monotonic NuGet package batch before publishing it."""

from __future__ import annotations

import base64
import json
import os
import re
import sys
import urllib.error
import urllib.parse
import urllib.request
import zipfile
from concurrent.futures import ThreadPoolExecutor
from dataclasses import dataclass
from functools import total_ordering
from pathlib import Path
from xml.etree import ElementTree


SEMVER = re.compile(
    r"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)"
    r"(?:-([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?"
    r"(?:\+[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?$"
)


@total_ordering
@dataclass(frozen=True)
class SemVer:
    major: int
    minor: int
    patch: int
    prerelease: tuple[str, ...] = ()

    def __lt__(self, other: object) -> bool:
        if not isinstance(other, SemVer):
            return NotImplemented
        core = (self.major, self.minor, self.patch)
        other_core = (other.major, other.minor, other.patch)
        if core != other_core:
            return core < other_core
        if not self.prerelease:
            return False
        if not other.prerelease:
            return True
        for left, right in zip(self.prerelease, other.prerelease, strict=False):
            if left == right:
                continue
            left_numeric = left.isdigit()
            right_numeric = right.isdigit()
            if left_numeric and right_numeric:
                return int(left) < int(right)
            if left_numeric != right_numeric:
                return left_numeric
            return left < right
        return len(self.prerelease) < len(other.prerelease)


@dataclass(frozen=True)
class PackedPackage:
    package_id: str
    version: str


def parse_semver(value: str) -> SemVer:
    match = SEMVER.fullmatch(value)
    if match is None:
        raise ValueError(f"Unsupported NuGet version '{value}'; expected SemVer 2.0")
    prerelease = tuple(part.lower() for part in (match.group(4) or "").split(".") if part)
    return SemVer(int(match.group(1)), int(match.group(2)), int(match.group(3)), prerelease)


def validate_batch(packages: list[PackedPackage], feed_versions: dict[str, list[str]]) -> str:
    if not packages:
        raise ValueError("No NuGet packages were produced")
    versions = {package.version for package in packages}
    if len(versions) != 1:
        raise ValueError(f"Expected one lockstep package version, found {sorted(versions)}")

    candidate_text = next(iter(versions))
    candidate = parse_semver(candidate_text)
    seen_ids: set[str] = set()
    for package in packages:
        package_key = package.package_id.lower()
        if package_key in seen_ids:
            raise ValueError(f"Package '{package.package_id}' was packed more than once")
        seen_ids.add(package_key)

        published = [parse_semver(version) for version in feed_versions[package_key]]
        if any(version == candidate for version in published):
            raise ValueError(f"{package.package_id}@{candidate_text} already exists in the immutable feed")
        if published and candidate <= max(published):
            raise ValueError(f"{package.package_id}@{candidate_text} does not advance its feed history")
    return candidate_text


def read_packed_packages(package_dir: Path) -> list[PackedPackage]:
    packages: list[PackedPackage] = []
    for package_path in sorted(package_dir.glob("*.nupkg")):
        with zipfile.ZipFile(package_path) as archive:
            nuspecs = [name for name in archive.namelist() if name.lower().endswith(".nuspec")]
            if len(nuspecs) != 1:
                raise ValueError(f"{package_path.name} contains {len(nuspecs)} nuspec files")
            root = ElementTree.fromstring(archive.read(nuspecs[0]))
        values = {
            element.tag.rsplit("}", 1)[-1]: (element.text or "").strip()
            for element in root.iter()
            if element.tag.rsplit("}", 1)[-1] in {"id", "version"}
        }
        if not values.get("id") or not values.get("version"):
            raise ValueError(f"{package_path.name} has no package id/version metadata")
        packages.append(PackedPackage(values["id"], values["version"]))
    return packages


def fetch_feed_versions(package_id: str, feed_download: str, username: str, token: str) -> list[str]:
    package_path = urllib.parse.quote(package_id.lower(), safe="")
    request = urllib.request.Request(f"{feed_download.rstrip('/')}/{package_path}/index.json")
    credentials = base64.b64encode(f"{username}:{token}".encode()).decode()
    request.add_header("Authorization", f"Basic {credentials}")
    try:
        with urllib.request.urlopen(request, timeout=30) as response:
            payload = json.load(response)
    except urllib.error.HTTPError as error:
        if error.code == 404:
            return []
        raise
    versions = payload.get("versions")
    if not isinstance(versions, list) or not all(isinstance(version, str) for version in versions):
        raise ValueError(f"Feed returned no version list for '{package_id}'")
    return versions


def main() -> int:
    if len(sys.argv) != 2:
        raise SystemExit("usage: package_publication_policy.py <nupkg-directory>")
    package_dir = Path(sys.argv[1])
    feed_download = os.environ["FEED_DOWNLOAD"]
    username = os.environ.get("FEED_USERNAME", "Concertable")
    token = os.environ["GITHUB_PACKAGES_TOKEN"]
    packages = read_packed_packages(package_dir)

    with ThreadPoolExecutor(max_workers=8) as executor:
        results = executor.map(
            lambda package: (
                package.package_id.lower(),
                fetch_feed_versions(package.package_id, feed_download, username, token),
            ),
            packages,
        )
        feed_versions = dict(results)

    version = validate_batch(packages, feed_versions)
    with Path(os.environ["GITHUB_OUTPUT"]).open("a", encoding="utf-8") as output:
        output.write(f"version={version}\n")
    print(f"Validated {len(packages)} fresh packages at lockstep version {version}.")
    return 0


if __name__ == "__main__":
    try:
        sys.exit(main())
    except (OSError, ValueError, urllib.error.URLError, zipfile.BadZipFile) as error:
        print(f"::error::{error}", file=sys.stderr)
        sys.exit(1)
