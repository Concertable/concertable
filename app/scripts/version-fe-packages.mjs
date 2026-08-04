import { execFileSync } from "node:child_process";
import { readFileSync, writeFileSync } from "node:fs";
import { resolve } from "node:path";

const writeVersions = process.argv[2] === "--write";
const packageDirectories = process.argv.slice(writeVersions ? 3 : 2);

if (packageDirectories.length === 0) {
  throw new Error("Pass at least one package directory");
}

const repositoryRoot = execFileSync("git", ["rev-parse", "--show-toplevel"], {
  encoding: "utf8",
}).trim();
const commitHeight = execFileSync("git", ["rev-list", "--count", "HEAD"], {
  encoding: "utf8",
}).trim();
const versions = [];

for (const packageDirectory of packageDirectories) {
  const packagePath = resolve(repositoryRoot, packageDirectory, "package.json");
  const packageJson = JSON.parse(readFileSync(packagePath, "utf8"));
  const baseVersion = packageJson.version.match(/^\d+\.\d+\.\d+/)?.[0];

  if (!baseVersion) {
    throw new Error(packageJson.name + " does not have a valid base version");
  }

  packageJson.version = baseVersion + "-alpha.0." + commitHeight;
  if (writeVersions) {
    writeFileSync(packagePath, JSON.stringify(packageJson, null, 2) + "\n");
  }
  versions.push(packageJson.version);
}

if (new Set(versions).size !== 1) {
  throw new Error("Frontend packages must share one lockstep version");
}

process.stdout.write(versions[0]);
