import { spawnSync } from "node:child_process";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const appRoot = dirname(dirname(fileURLToPath(import.meta.url)));
const executable = join(appRoot, "node_modules", ".bin", process.platform === "win32" ? "depcruise.cmd" : "depcruise");
const workspaces = [
  ["web/customer", "web/customer/tsconfig.app.json"],
  ["web/b2b/venue", "web/b2b/venue/tsconfig.app.json"],
  ["web/b2b/artist", "web/b2b/artist/tsconfig.app.json"],
  ["web/b2b/business", "web/b2b/business/tsconfig.app.json"],
  ["mobile/customer", "mobile/customer/tsconfig.json"],
  ["mobile/b2b", "mobile/b2b/tsconfig.json"],
  ["shared", "shared/tsconfig.build.json"],
  ["web/shared", "web/shared/tsconfig.build.json"],
  ["web/b2b/shared", "web/b2b/shared/tsconfig.build.json"],
  ["mobile/shared", "mobile/shared/tsconfig.build.json"],
  ["customer/shared", "customer/shared/tsconfig.build.json"],
];

let failed = false;
for (const [workspace, tsConfig] of workspaces) {
  const result = spawnSync(
    executable,
    [workspace, "--config", ".dependency-cruiser.cjs", "--ts-config", join(appRoot, tsConfig), "--output-type", "err"],
    { cwd: appRoot, encoding: "utf8" },
  );
  process.stdout.write(result.stdout ?? "");
  process.stderr.write(result.stderr ?? "");
  failed ||= result.status !== 0;
}

process.exitCode = failed ? 1 : 0;
