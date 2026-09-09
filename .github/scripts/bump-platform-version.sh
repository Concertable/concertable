#!/usr/bin/env bash
# Bump every service's <ConcertableDotNetPlatformVersion> pin to a target version.
#
# Each service folder owns its own Directory.Packages.props with a single
# <ConcertableDotNetPlatformVersion> line (the lockstep pin for all Concertable.* feed packages).
# This sets every one that defines the pin to $1, and prints the files it changed — one per
# line — so the caller (platform-sync.yml) knows whether there is anything to open a PR for.
# Idempotent: a file already at the target is left untouched and not printed.
#
# Usage: bump-platform-version.sh <version>
#   e.g. bump-platform-version.sh 0.1.0-alpha.0.547
set -euo pipefail

target="${1:?usage: bump-platform-version.sh <version>}"
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"

mapfile -t files < <(grep -rlE '<ConcertableDotNetPlatformVersion>[^<]+</ConcertableDotNetPlatformVersion>' \
  "$root/api" --include=Directory.Packages.props | sort)

[ "${#files[@]}" -gt 0 ] || { echo "no ConcertableDotNetPlatformVersion pins found under api/ — refusing" >&2; exit 1; }

for f in "${files[@]}"; do
  current="$(sed -nE 's:.*<ConcertableDotNetPlatformVersion>([^<]+)</ConcertableDotNetPlatformVersion>.*:\1:p' "$f")"
  [ "$current" = "$target" ] && continue
  sed -i -E "s:(<ConcertableDotNetPlatformVersion>)[^<]+(</ConcertableDotNetPlatformVersion>):\1${target}\2:" "$f"
  echo "$f"
done
