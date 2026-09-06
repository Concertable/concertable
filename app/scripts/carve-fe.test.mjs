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

test("an exact package version replaces every Concertable dependency", () => {
  const exactVersion = "0.1.0-alpha.0.6272";
  const result = spawnSync(
    process.execPath,
    [
      "scripts/carve-fe.mjs",
      "mobile/b2b",
      `--package-version=${exactVersion}`,
      "--prepare-only",
      "--keep",
    ],
    { cwd: appRoot, encoding: "utf8" },
  );
  const output = `${result.stdout ?? ""}\n${result.stderr ?? ""}`;
  assert.equal(result.status, 0, output);

  const workMatch = output.match(/\(kept carve work: (.+)\)/);
  assert.ok(workMatch, output);
  const work = resolve(workMatch[1].trim());

  try {
    const packageJson = JSON.parse(
      readFileSync(join(work, "repo", "app", "mobile", "b2b", "package.json"), "utf8"),
    );
    const concertableVersions = Object.entries(packageJson.dependencies)
      .filter(([name]) => name.startsWith("@concertable/"))
      .map(([, version]) => version);
    assert.deepEqual(concertableVersions, [exactVersion, exactVersion, exactVersion]);
  } finally {
    rmSync(work, { recursive: true, force: true });
  }
});

test("a moving tag or range cannot masquerade as an exact package version", () => {
  const result = spawnSync(
    process.execPath,
    [
      "scripts/carve-fe.mjs",
      "mobile/b2b",
      "--package-version=^0.1.0",
      "--prepare-only",
    ],
    { cwd: appRoot, encoding: "utf8" },
  );

  assert.notEqual(result.status, 0);
  assert.match(
    `${result.stdout ?? ""}\n${result.stderr ?? ""}`,
    /must be an exact npm version/,
  );
});
