import { spawnSync } from "node:child_process";

const packageVersion = process.env.B2B_PHASE3_PRODUCER_VERSION;
if (!packageVersion) {
  throw new Error(
    "B2B_PHASE3_PRODUCER_VERSION must name the exact feed-verified producer version.",
  );
}

for (const surface of ["web/b2b/venue", "web/b2b/artist", "mobile/b2b"]) {
  const result = spawnSync(
    process.execPath,
    [
      "scripts/carve-fe.mjs",
      surface,
      `--package-version=${packageVersion}`,
    ],
    { stdio: "inherit" },
  );
  if (result.error) throw result.error;
  if (result.status !== 0)
    throw new Error(`Exact-version carve failed for ${surface}.`);
}
