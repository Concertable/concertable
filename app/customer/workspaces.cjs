module.exports = {
  workspaces: [
    ["customer/shared", "customer/shared/tsconfig.build.json"],
    ["web/customer", "web/customer/tsconfig.app.json"],
    ["mobile/customer", "mobile/customer/tsconfig.json"],
  ],
};
