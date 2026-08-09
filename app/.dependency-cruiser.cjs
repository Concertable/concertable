const WORKSPACES = [
  "web/b2b/shared",
  "web/b2b/venue",
  "web/b2b/artist",
  "web/b2b/business",
  "web/customer",
  "web/shared",
  "mobile/customer",
  "mobile/b2b",
  "mobile/shared",
  "customer/shared",
  "shared",
].join("|");

module.exports = {
  forbidden: [
    {
      name: "not-to-foreign-workspace",
      severity: "error",
      from: { path: `^(${WORKSPACES})/` },
      to: {
        path: `^(${WORKSPACES})/`,
        pathNot: ["^$1/"],
      },
    },
  ],
  options: {
    tsPreCompilationDeps: true,
    doNotFollow: { path: "node_modules" },
    exclude: { path: "node_modules|/dist/|\\.d\\.ts$" },
    includeOnly: { path: "^(web|mobile|shared|customer)(/|$)" },
    preserveSymlinks: true,
  },
};
