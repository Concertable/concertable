import assert from "node:assert/strict";
import { existsSync, readFileSync, rmSync } from "node:fs";
import { tmpdir } from "node:os";
import { basename, dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { spawnSync } from "node:child_process";
import test from "node:test";

const appRoot = dirname(dirname(fileURLToPath(import.meta.url)));
const webSurfaces = [
  "web/customer",
  "web/admin",
  "web/b2b/venue",
  "web/b2b/artist",
  "web/b2b/business",
];
const mobileSurfaces = ["mobile/b2b", "mobile/customer"];

for (const surface of webSurfaces) {
  test(`${surface} carve resolves its shared Vite HTTPS helper`, () => {
    const result = spawnSync(
      process.execPath,
      ["scripts/carve-fe.mjs", surface, "--prepare-only", "--keep"],
      { cwd: appRoot, encoding: "utf8" },
    );
    const output = `${result.stdout ?? ""}\n${result.stderr ?? ""}`;
    assert.equal(result.status, 0, output);

    const workMatch = output.match(/\(kept carve work: (.+)\)/);
    assert.ok(workMatch, output);
    const work = resolve(workMatch[1].trim());
    assert.equal(dirname(work), resolve(tmpdir()));
    assert.match(basename(work), /^carve-fe-/);

    try {
      const surfaceDirectory = join(work, "repo", "app", ...surface.split("/"));
      const configPath = join(surfaceDirectory, "vite.config.ts");
      const config = readFileSync(configPath, "utf8");
      const importMatch = config.match(/from ['"](.+\/vite-development-https)['"]/);
      assert.ok(importMatch, `${configPath} does not import the shared HTTPS helper`);
      assert.equal(existsSync(resolve(surfaceDirectory, `${importMatch[1]}.ts`)), true);
    } finally {
      rmSync(work, { recursive: true, force: true });
    }
  });
}

for (const surface of mobileSurfaces) {
  test(`${surface} carve consumes every shared tier from the feed`, () => {
    const result = spawnSync(
      process.execPath,
      ["scripts/carve-fe.mjs", surface, "--prepare-only", "--keep"],
      { cwd: appRoot, encoding: "utf8" },
    );
    const output = `${result.stdout ?? ""}\n${result.stderr ?? ""}`;
    assert.equal(result.status, 0, output);

    const workMatch = output.match(/\(kept carve work: (.+)\)/);
    assert.ok(workMatch, output);
    const work = resolve(workMatch[1].trim());
    assert.equal(dirname(work), resolve(tmpdir()));
    assert.match(basename(work), /^carve-fe-/);

    try {
      const surfaceDirectory = join(work, "repo", "app", ...surface.split("/"));
      const manifest = JSON.parse(readFileSync(join(surfaceDirectory, "package.json"), "utf8"));
      const specifiers = { ...manifest.dependencies, ...manifest.devDependencies };
      for (const [name, specifier] of Object.entries(specifiers)) {
        if (name.startsWith("@concertable/")) assert.equal(specifier, "alpha", name);
      }
      assert.equal(specifiers["@concertable/build-config"], "alpha");
      assert.match(readFileSync(join(surfaceDirectory, "metro.config.js"), "utf8"),
        /@concertable\/build-config\/metro/);
      assert.equal(existsSync(join(work, "repo", "app", "build-config")), false);
      assert.equal(existsSync(join(work, "repo", "app", "mobile", "shared")), false);
    } finally {
      rmSync(work, { recursive: true, force: true });
    }
  });
}

// Publication is lockstep across all seven tiers, so a lock sitting on an older version means its
// surface was skipped by `npm run lock:carve`.
test("every carved surface pins one lockstep tier version from the feed", () => {
  const pinned = new Map();

  for (const surface of [...webSurfaces, ...mobileSurfaces]) {
    const lockPath = join(appRoot, ...surface.split("/"), "package-lock.json");
    assert.equal(existsSync(lockPath), true, `${surface} has no standalone lockfile`);
    const lock = JSON.parse(readFileSync(lockPath, "utf8"));

    const declared = lock.packages[""];
    for (const field of ["dependencies", "devDependencies"]) {
      for (const [name, specifier] of Object.entries(declared[field] ?? {})) {
        if (name.startsWith("@concertable/")) assert.equal(specifier, "alpha", `${surface} ${name}`);
      }
    }

    for (const [entryPath, entry] of Object.entries(lock.packages)) {
      const name = entryPath.replace(/^.*node_modules\//, "");
      if (!name.startsWith("@concertable/")) continue;
      assert.match(entry.resolved, /^https:\/\/npm\.pkg\.github\.com\//, `${surface} ${name}`);
      pinned.set(`${surface} ${name}`, entry.version);
    }
  }

  assert.ok(pinned.size > 0);
  assert.equal(new Set(pinned.values()).size, 1,
    `tiers are not on one lockstep version: ${JSON.stringify([...pinned], null, 2)}`);
});
