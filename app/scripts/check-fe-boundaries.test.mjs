import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { existsSync, rmSync, writeFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";
import test from "node:test";

const appRoot = dirname(dirname(fileURLToPath(import.meta.url)));
const probe = join(appRoot, "web/customer/src/__boundary_probe__.ts");
const b2bProbe = join(appRoot, "b2b/shared/src/__boundary_probe__.ts");

test("rejects imports from another surface and a tier's source", () => {
  assert.equal(existsSync(probe), false);
  writeFileSync(
    probe,
    'import "../../b2b/venue/src/main";\nimport "../../../shared/src/index";\n',
  );

  let result;
  try {
    result = spawnSync(
      process.execPath,
      ["scripts/check-fe-boundaries.mjs"],
      { cwd: appRoot, encoding: "utf8" },
    );
  } finally {
    rmSync(probe, { force: true });
  }

  const output = `${result.stdout ?? ""}\n${result.stderr ?? ""}`;
  assert.notEqual(result.status, 0, output);
  assert.match(output, /not-to-foreign-workspace/);
  assert.match(output, /2 dependency violations/);
});

test("rejects platform source imports from the cross-platform B2B tier", () => {
  assert.equal(existsSync(b2bProbe), false);
  writeFileSync(b2bProbe, 'import "../../../web/shared/src/lib/queryClient";\n');

  let result;
  try {
    result = spawnSync(
      process.execPath,
      ["scripts/check-fe-boundaries.mjs"],
      { cwd: appRoot, encoding: "utf8" },
    );
  } finally {
    rmSync(b2bProbe, { force: true });
  }

  const output = `${result.stdout ?? ""}\n${result.stderr ?? ""}`;
  assert.notEqual(result.status, 0, output);
  assert.match(output, /cross-platform-b2b-has-no-platform-dependencies/);
});

test("rejects misplaced feature type and runtime imports", () => {
  assert.equal(existsSync(probe), false);
  writeFileSync(
    probe,
    'import { useVenue, type Venue } from "@concertable/shared/features/venues";\nexport { type Venue as ExportedVenue } from "@concertable/shared/features/venues";\nimport { useVenue as wrongUseVenue } from "@concertable/shared/features/venues/types";\nvoid useVenue;\nvoid wrongUseVenue;\nvoid (null as Venue | null);\n',
  );

  let result;
  try {
    result = spawnSync(
      process.execPath,
      ["scripts/check-fe-boundaries.mjs"],
      { cwd: appRoot, encoding: "utf8" },
    );
  } finally {
    rmSync(probe, { force: true });
  }

  const output = `${result.stdout ?? ""}\n${result.stderr ?? ""}`;
  assert.notEqual(result.status, 0, output);
  assert.match(output, /feature-type-import-requires-types-entrypoint/);
  assert.match(output, /feature-runtime-import-requires-feature-entrypoint/);
});
