import { sourceAlias } from "./vite.mjs";

export function nodeTests(rootDirectory) {
  return {
    resolve: { alias: [sourceAlias(rootDirectory)] },
    test: { environment: "node", include: ["src/**/*.test.ts"] },
  };
}
