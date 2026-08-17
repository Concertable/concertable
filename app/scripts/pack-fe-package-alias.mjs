import { cpSync, existsSync, mkdirSync, mkdtempSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { basename, dirname, isAbsolute, join, relative, resolve } from "node:path";
import { pathToFileURL } from "node:url";
import { spawnSync } from "node:child_process";

function npmCommand() {
  if (process.platform === "win32") {
    return {
      command: process.execPath,
      arguments: [join(dirname(process.execPath), "node_modules", "npm", "bin", "npm-cli.js")],
    };
  }

  return { command: "npm", arguments: [] };
}

function copyPublishedFiles(sourceDirectory, stagingDirectory, packageJson) {
  if (!Array.isArray(packageJson.files) || packageJson.files.length === 0) {
    throw new Error(`${packageJson.name} must declare the files included in its package`);
  }

  const sourceRoot = resolve(sourceDirectory);
  for (const entry of packageJson.files) {
    const source = resolve(sourceRoot, entry);
    const packagePath = relative(sourceRoot, source);

    if (!packagePath || packagePath.startsWith("..") || isAbsolute(packagePath) || !existsSync(source)) {
      throw new Error(`Cannot stage package file entry: ${entry}`);
    }

    const destination = join(stagingDirectory, packagePath);
    mkdirSync(dirname(destination), { recursive: true });
    cpSync(source, destination, { recursive: true });
  }

  for (const name of ["README", "README.md", "LICENSE", "LICENSE.md"]) {
    const source = join(sourceDirectory, name);
    if (existsSync(source)) {
      cpSync(source, join(stagingDirectory, basename(source)));
    }
  }
}

export function packFrontendPackageAlias(sourceDirectory, aliasName, destinationDirectory) {
  const source = resolve(sourceDirectory);
  const destination = resolve(destinationDirectory);
  const packageJsonPath = join(source, "package.json");
  const packageJson = JSON.parse(readFileSync(packageJsonPath, "utf8"));

  if (!/^@[a-z0-9-]+\/[a-z0-9-]+$/.test(aliasName)) {
    throw new Error(`Invalid scoped package alias: ${aliasName}`);
  }

  if (packageJson.name === aliasName) {
    throw new Error(`${aliasName} is already the source package name`);
  }

  mkdirSync(destination, { recursive: true });
  const stagingRoot = mkdtempSync(join(tmpdir(), "concertable-fe-package-alias-"));
  const stagingDirectory = join(stagingRoot, "package");
  mkdirSync(stagingDirectory);

  try {
    copyPublishedFiles(source, stagingDirectory, packageJson);
    writeFileSync(
      join(stagingDirectory, "package.json"),
      `${JSON.stringify({ ...packageJson, name: aliasName }, null, 2)}\n`,
    );

    const npm = npmCommand();
    const result = spawnSync(
      npm.command,
      [
        ...npm.arguments,
        "pack",
        stagingDirectory,
        "--ignore-scripts",
        "--pack-destination",
        destination,
        "--json",
      ],
      { encoding: "utf8" },
    );

    if (result.error) {
      throw result.error;
    }

    if (result.status !== 0) {
      throw new Error(result.stderr || `npm pack exited with code ${result.status}`);
    }

    const [packed] = JSON.parse(result.stdout);
    if (!packed?.filename) {
      throw new Error("npm pack did not return a tarball filename");
    }

    return join(destination, packed.filename);
  } finally {
    rmSync(stagingRoot, { recursive: true, force: true });
  }
}

const invokedPath = process.argv[1] ? pathToFileURL(resolve(process.argv[1])).href : undefined;
if (invokedPath === import.meta.url) {
  const [, , sourceDirectory, aliasName, destinationDirectory] = process.argv;

  if (!sourceDirectory || !aliasName || !destinationDirectory) {
    throw new Error(
      "Usage: node pack-fe-package-alias.mjs <source-directory> <alias-name> <destination-directory>",
    );
  }

  process.stdout.write(
    `${packFrontendPackageAlias(sourceDirectory, aliasName, destinationDirectory)}\n`,
  );
}
