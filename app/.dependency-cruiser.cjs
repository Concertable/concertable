const createDependencyCruiserConfig = require("@concertable/build-config/dependency-cruiser");
const { workspaces, forbidden } = require("./workspaces.cjs");

module.exports = createDependencyCruiserConfig({
  workspaces: workspaces.map(([workspace]) => workspace),
  forbidden,
});
