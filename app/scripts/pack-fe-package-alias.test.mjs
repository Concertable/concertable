import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { mkdirSync, mkdtempSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { dirname, join } from "node:path";
import test from "node:test";
import { packFrontendPackageAlias } from "./pack-fe-package-alias.mjs";

function runNpm(args, cwd) {
  const command = process.platform === "win32" ? process.execPath : "npm";
  const prefix =
    process.platform === "win32"
      ? [join(dirname(process.execPath), "node_modules", "npm", "bin", "npm-cli.js")]
      : [];
  const result = spawnSync(command, [...prefix, ...args], { cwd, encoding: "utf8" });

  assert.equal(result.status, 0, result.stderr || result.stdout);
}

test("packs an alias without changing the source package", () => {
  const root = mkdtempSync(join(tmpdir(), "concertable-fe-package-alias-test-"));
  const source = join(root, "source");
  const destination = join(root, "packages");
  const consumer = join(root, "consumer");
  const sourceManifest = {
    name: "@concertable/original",
    version: "1.2.3",
    files: ["dist"],
    scripts: { prepack: 'node -e "process.exit(91)"' },
  };

  try {
    mkdirSync(join(source, "dist"), { recursive: true });
    mkdirSync(consumer);
    writeFileSync(join(source, "package.json"), `${JSON.stringify(sourceManifest, null, 2)}\n`);
    writeFileSync(join(source, "dist", "index.js"), "export const value = 42;\n");
    writeFileSync(
      join(consumer, "package.json"),
      `${JSON.stringify({ name: "alias-consumer", private: true }, null, 2)}\n`,
    );

    const tarball = packFrontendPackageAlias(
      source,
      "@concertable/web-original",
      destination,
    );
    runNpm(
      ["install", "--ignore-scripts", "--no-audit", "--no-fund", "--no-package-lock", tarball],
      consumer,
    );

    const installedManifest = JSON.parse(
      readFileSync(
        join(consumer, "node_modules", "@concertable", "web-original", "package.json"),
        "utf8",
      ),
    );

    assert.equal(installedManifest.name, "@concertable/web-original");
    assert.equal(installedManifest.version, sourceManifest.version);
    assert.deepEqual(installedManifest.scripts, sourceManifest.scripts);
    assert.equal(
      readFileSync(
        join(consumer, "node_modules", "@concertable", "web-original", "dist", "index.js"),
        "utf8",
      ),
      "export const value = 42;\n",
    );
    assert.deepEqual(JSON.parse(readFileSync(join(source, "package.json"), "utf8")), sourceManifest);
  } finally {
    rmSync(root, { recursive: true, force: true });
  }
});
