const { existsSync } = require("node:fs");
const { createRequire } = require("node:module");
const { basename, dirname, join } = require("node:path");

module.exports = function withPackageResolution(config, projectDirectory, packageNames) {
  const projectRequire = createRequire(join(projectDirectory, "package.json"));
  const packageDirectories = packageNames.map((name) =>
    dirname(projectRequire.resolve(`${name}/package.json`)),
  );
  const nodeModulesDirectories = [...new Set(
    [projectDirectory, ...packageDirectories].flatMap((directory) => {
      const packageRequire = createRequire(join(directory, "package.json"));
      return packageRequire.resolve.paths("__metro_dependency__")
        .filter((candidate) => basename(candidate) === "node_modules" && existsSync(candidate));
    }),
  )];

  return {
    ...config,
    watchFolders: [...new Set([
      ...(config.watchFolders ?? []),
      ...packageDirectories,
      ...nodeModulesDirectories,
    ])],
    resolver: {
      ...config.resolver,
      nodeModulesPaths: [...new Set([
        ...(config.resolver.nodeModulesPaths ?? []),
        ...nodeModulesDirectories,
      ])],
    },
  };
};
