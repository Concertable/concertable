import { execFileSync, spawnSync } from "node:child_process";
import { mkdirSync, mkdtempSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";

// Carve one frontend surface into a standalone tree and build it against the package feed ONLY — the
// npm counterpart of the backend carve-{auth,payment,search,b2b,customer} CI gates. A surface that
// imports an @concertable tier it does not declare (masked in-monorepo by workspace hoisting) fails
// here at install; a shared import absent from the feed fails at restore; a build that only resolves
// via monorepo-root config fails standalone.
//
//   node scripts/carve-fe.mjs <surface> [--worktree] [--keep]
//
// Requires GITHUB_PACKAGES_TOKEN (a PAT with read:packages) in the environment — same credential the
// feed restore uses everywhere else.

// Build kind + the VITE_* placeholders a carved web build needs: each surface's vite.config reads env
// from a parent dir outside the carved tree and, in build mode, calls .replace() on the API URL, so a
// standalone build crashes without them. The value is never dereferenced — only shaped.
const SURFACES = {
  "web/customer": { kind: "web", env: { VITE_CUSTOMER_API_URL: "https://carve.invalid/api" } },
  "web/b2b/venue": { kind: "web", env: { VITE_B2B_API_URL: "https://carve.invalid/api" } },
  "web/b2b/artist": { kind: "web", env: { VITE_B2B_API_URL: "https://carve.invalid/api" } },
  "web/b2b/business": { kind: "web", env: { VITE_B2B_API_URL: "https://carve.invalid/api" } },
  "mobile/customer": { kind: "mobile", env: {} },
  "mobile/b2b": { kind: "mobile", env: {} },
};

const argv = process.argv.slice(2);
const surface = argv.find((a) => !a.startsWith("--"));
const useWorktree = argv.includes("--worktree");
const keep = argv.includes("--keep");

if (!surface || !SURFACES[surface]) {
  throw new Error(`Usage: node carve-fe.mjs <${Object.keys(SURFACES).join("|")}> [--worktree] [--keep]`);
}
if (!process.env.GITHUB_PACKAGES_TOKEN) {
  throw new Error("GITHUB_PACKAGES_TOKEN is required (a PAT with read:packages).");
}

const spec = SURFACES[surface];
const repoRoot = execFileSync("git", ["rev-parse", "--show-toplevel"], { encoding: "utf8" }).trim();

// HEAD matches CI on the pushed commit; --worktree carves uncommitted working-tree state for local
// pre-commit iteration via a throwaway stash object (no effect on the index or working tree).
let treeish = "HEAD";
if (useWorktree) {
  const wip = execFileSync("git", ["stash", "create"], { cwd: repoRoot, encoding: "utf8" }).trim();
  treeish = wip || "HEAD";
}

const npm = process.platform === "win32" ? process.execPath : "npm";
const npmPrefix =
  process.platform === "win32"
    ? [join(process.execPath, "..", "node_modules", "npm", "bin", "npm-cli.js")]
    : [];

const work = mkdtempSync(join(tmpdir(), "carve-fe-"));
const dir = join(work, "surface");
// Fresh per-run cache by default (CI isolation); a persistent path speeds local iteration.
const cache = process.env.CARVE_NPM_CACHE || join(work, "npm-cache");

function run(cmd, args, opts = {}) {
  const r = spawnSync(cmd, args, { stdio: "inherit", ...opts });
  if (r.error) throw r.error;
  if (r.status !== 0) throw new Error(`${cmd} ${args.join(" ")} exited ${r.status}`);
}

try {
  // 1. Extract only the surface's tracked tree (no siblings, no .git) — BE parity: git archive, not
  //    subtree-split. `<treeish>:<path>` yields the subtree; tar keeps it cross-platform (Linux CI).
  const tar = join(work, "surface.tar");
  execFileSync("git", ["archive", "--format=tar", "-o", tar, `${treeish}:app/${surface}`], {
    cwd: repoRoot,
  });
  mkdirSync(dir, { recursive: true });
  // Relative paths under cwd — an absolute Windows path (C:\...) makes GNU tar read the drive as a
  // remote host ("Cannot connect to C:"). Portable across the Linux CI and Windows tars.
  run("tar", ["-xf", "surface.tar", "-C", "surface"], { cwd: work });

  // 2. Rewrite intra-@concertable specifiers "*" -> "alpha": "*" links the workspace copy in-monorepo
  //    but is unresolvable from the feed (the tiers publish only alpha-tagged prereleases). The tag
  //    resolves the current lockstep publish; the surface's own source is unchanged.
  const pkgPath = join(dir, "package.json");
  const pkg = JSON.parse(readFileSync(pkgPath, "utf8"));
  for (const field of ["dependencies", "devDependencies", "peerDependencies", "optionalDependencies"]) {
    const deps = pkg[field];
    if (!deps) continue;
    for (const name of Object.keys(deps)) {
      if (name.startsWith("@concertable/")) deps[name] = "alpha";
    }
  }
  writeFileSync(pkgPath, JSON.stringify(pkg, null, 2) + "\n");

  // 3. Self-contained feed config — nothing from the monorepo root leaks in (BE per-folder-config rule).
  writeFileSync(
    join(dir, ".npmrc"),
    [
      "@concertable:registry=https://npm.pkg.github.com",
      "//npm.pkg.github.com/:_authToken=${GITHUB_PACKAGES_TOKEN}",
      "always-auth=true",
      "legacy-peer-deps=true",
      `cache=${cache}`,
      "",
    ].join("\n"),
  );

  // 4. Restore from the feed only — no workspace root above the temp dir to resolve @concertable/* from.
  run(npm, [...npmPrefix, "install", "--no-audit", "--no-fund"], { cwd: dir });

  // 5. Build the surface standalone.
  if (spec.kind === "web") {
    run(npm, [...npmPrefix, "run", "build"], { cwd: dir, env: { ...process.env, ...spec.env } });
  } else {
    run(npm, [...npmPrefix, "exec", "--", "tsc", "--noEmit", "-p", "tsconfig.json"], { cwd: dir });
  }

  console.log(`\n>>> carve-fe ${surface}: standalone restore + build OK`);
} finally {
  if (keep) console.log(`\n(kept carved tree: ${dir})`);
  else rmSync(work, { recursive: true, force: true });
}
