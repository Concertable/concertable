#!/usr/bin/env bash

set -u

if [ "$#" -ne 1 ]; then
    echo "Usage: run-repo-hook.sh <hook.py>" >&2
    exit 1
fi

repo_root=$(git rev-parse --show-toplevel) || exit 1
script="$repo_root/.agents/hooks/$1"
case "$(uname -s)" in
    CYGWIN*|MINGW*|MSYS*)
        script=$(cygpath -w "$script") || exit 1
        exec python -B "$script"
        ;;
    *) exec python3 -B "$script" ;;
esac
