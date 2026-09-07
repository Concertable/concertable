import path from "node:path";

export function sourceAlias(rootDirectory) {
  return { find: "@", replacement: path.resolve(rootDirectory, "src") };
}
