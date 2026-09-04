const b2b = require("./b2b/workspaces.cjs");
const customer = require("./customer/workspaces.cjs");

module.exports = {
  workspaces: [
    ["shared", "shared/tsconfig.build.json"],
    ["web/shared", "web/shared/tsconfig.build.json"],
    ["mobile/shared", "mobile/shared/tsconfig.build.json"],
    ...b2b.workspaces,
    ...customer.workspaces,
  ],
  forbidden: b2b.forbidden,
};
