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
