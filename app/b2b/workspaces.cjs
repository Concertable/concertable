module.exports = {
  workspaces: [
    ["b2b/shared", "b2b/shared/tsconfig.build.json"],
    ["web/b2b/shared", "web/b2b/shared/tsconfig.build.json"],
    ["web/b2b/venue", "web/b2b/venue/tsconfig.app.json"],
    ["web/b2b/artist", "web/b2b/artist/tsconfig.app.json"],
    ["web/b2b/business", "web/b2b/business/tsconfig.app.json"],
    ["web/admin", "web/admin/tsconfig.app.json"],
    ["mobile/b2b", "mobile/b2b/tsconfig.json"],
  ],
  forbidden: [
    {
      name: "cross-platform-b2b-has-no-platform-dependencies",
      severity: "error",
      from: { path: "^b2b/shared/" },
      to: {
        path: "^(web|mobile)/|node_modules/(@concertable/web|@tanstack/react-router|sonner|react-dom|expo-secure-store)(/|$)",
      },
    },
  ],
};
