const escapePattern = (value) => value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");

module.exports = function createDependencyCruiserConfig({ workspaces, forbidden = [] }) {
  if (!workspaces.length) {
    throw new Error("At least one workspace is required for boundary enforcement.");
  }

  const paths = workspaces.map(escapePattern).join("|");
  return {
    forbidden: [
      {
        name: "not-to-foreign-workspace",
        severity: "error",
        from: { path: `^(${paths})/` },
        to: { path: `^(${paths})/`, pathNot: ["^$1/"] },
      },
      ...forbidden,
    ],
    options: {
      tsPreCompilationDeps: true,
      doNotFollow: { path: "node_modules" },
      exclude: { path: "node_modules|/dist/|\\.d\\.ts$" },
      includeOnly: { path: `^(${paths})(/|$)` },
      preserveSymlinks: true,
    },
  };
};
