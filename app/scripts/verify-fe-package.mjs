import { mkdirSync, mkdtempSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { dirname, join } from "node:path";
import { spawnSync } from "node:child_process";

const installTarget = process.argv[2];
const packageName = process.argv[3];
const metroOnly = process.argv.includes("--metro-only");
const verifyMetro = metroOnly || process.argv.includes("--metro");
const verifyNode = !metroOnly;

if (!installTarget || !packageName) {
  throw new Error(
    "Usage: node verify-fe-package.mjs <install-target> <package-name> [--metro|--metro-only]",
  );
}

// One representative export per published tier. Node checks stay light (a pure util / constant /
// type) so a throwaway NodeNext consumer resolves the tier plus its @concertable deps from the feed
// without dragging heavy component transitive types in. Mobile is metro-only (react-native runtime).
function b2bChecks(name, tenantExport = "features/tenant") {
  const tenantModule = `${name}/${tenantExport}`;

  return {
    node: [
      `import { TENANT_HEADER } from "${tenantModule}";`,
      `import type { TenantRole } from "${name}/features/tenant/types";`,
      `if (TENANT_HEADER !== "X-Tenant-Id") throw new Error("Unexpected ${name} TENANT_HEADER");`,
      `const role = "Admin" as TenantRole;`,
      "void role;",
    ],
    metro: [
      'import { registerRootComponent } from "expo";',
      'import React from "react";',
      'import { Text } from "react-native";',
      `import { TENANT_HEADER } from "${tenantModule}";`,
      "function App() {",
      "  return React.createElement(Text, null, TENANT_HEADER);",
      "}",
      "registerRootComponent(App);",
    ],
  };
}

const CHECKS = {
  "@concertable/shared": {
    node: [
      'import { GENRE_LABELS, genreLabel } from "@concertable/shared";',
      'import { useMountEffect } from "@concertable/shared/hooks/useMountEffect";',
      'import type { Genre } from "@concertable/shared/types";',
      'import { useAuthStore } from "@concertable/shared/features/auth";',
      'import type { User } from "@concertable/shared/features/auth/types";',
      'const genre: Genre = "rock";',
      'if (genreLabel(genre) !== "Rock") throw new Error("Unexpected genre label");',
      'if (GENRE_LABELS[genre] !== "Rock") throw new Error("Unexpected genre labels");',
      'if (typeof useMountEffect !== "function") throw new Error("Missing useMountEffect export");',
      'if (typeof useAuthStore !== "function") throw new Error("Missing useAuthStore export");',
      "const user = {} as User;",
      "void user;",
    ],
    nodeRuntime: [
      'import { GENRE_LABELS, genreLabel } from "@concertable/shared";',
      'import { useMountEffect } from "@concertable/shared/hooks/useMountEffect";',
      'import { useAuthStore } from "@concertable/shared/features/auth";',
      'if (genreLabel("rock") !== "Rock") throw new Error("Unexpected genre label");',
      'if (GENRE_LABELS.rock !== "Rock") throw new Error("Unexpected genre labels");',
      'if (typeof useMountEffect !== "function") throw new Error("Missing useMountEffect export");',
      'if (typeof useAuthStore !== "function") throw new Error("Missing useAuthStore export");',
    ],
    metro: [
      'import { registerRootComponent } from "expo";',
      'import React from "react";',
      'import { Text } from "react-native";',
      'import { useAuthStore } from "@concertable/shared/features/auth";',
      'import { useMountEffect } from "@concertable/shared/hooks/useMountEffect";',
      'import { genreLabel } from "@concertable/shared/types";',
      "function App() {",
      "  useMountEffect(() => undefined);",
      "  void useAuthStore;",
      '  return React.createElement(Text, null, genreLabel("rock"));',
      "}",
      "registerRootComponent(App);",
    ],
  },
  "@concertable/web": {
    node: [
      'import { cn } from "@concertable/web/lib/utils";',
      'import type { User } from "@concertable/web/features/auth/types";',
      'import { ReviewRouteProvider, b2bReviewBasePath, customerReviewBasePath } from "@concertable/web/features/reviews";',
      'import { useMeQuery } from "@concertable/web/features/user";',
      'if (typeof cn !== "function") throw new Error("Missing @concertable/web cn export");',
      'if (typeof ReviewRouteProvider !== "function") throw new Error("Missing review route provider export");',
      'if (typeof useMeQuery !== "function") throw new Error("Missing useMeQuery export");',
      'const user = {} as User;',
      'void user;',
      'if (b2bReviewBasePath("artist", 12) !== "/artist/12/review") throw new Error("Unexpected B2B review route");',
      'if (customerReviewBasePath("artist", 12) !== "/artists/12/reviews") throw new Error("Unexpected customer review route");',
    ],
  },
  "@concertable/customer": {
    node: [
      'import { customerClient } from "@concertable/customer/lib/customerClient";',
      'import type { CreateReviewRequest } from "@concertable/customer/features/reviews/types";',
      'if (!customerClient) throw new Error("Missing @concertable/customer customerClient export");',
      'const request = {} as CreateReviewRequest;',
      'void request;',
    ],
  },
  "@concertable/b2b": b2bChecks("@concertable/b2b"),
  "@concertable/web-b2b": b2bChecks("@concertable/web-b2b", "features/tenant/constants"),
  "@concertable/mobile": {
    metro: [
      'import { registerRootComponent } from "expo";',
      'import React from "react";',
      'import { Text } from "react-native";',
      'import { cn } from "@concertable/mobile/lib/utils";',
      'function App() {',
      '  return React.createElement(Text, null, cn("a", "b"));',
      "}",
      "registerRootComponent(App);",
    ],
  },
};

const checks = CHECKS[packageName];
if (!checks) {
  throw new Error(`No verification profile for ${packageName}`);
}

const npm = process.platform === "win32" ? process.execPath : "npm";
const npmArguments =
  process.platform === "win32"
    ? [join(dirname(process.execPath), "node_modules", "npm", "bin", "npm-cli.js")]
    : [];
const consumerRoot = mkdtempSync(join(tmpdir(), "concertable-fe-package-"));

function writeJson(directory, name, value) {
  writeFileSync(join(directory, name), `${JSON.stringify(value, null, 2)}\n`);
}

function run(args, cwd) {
  const result = spawnSync(npm, [...npmArguments, ...args], { cwd, stdio: "inherit" });

  if (result.error) {
    throw result.error;
  }

  if (result.status !== 0) {
    throw new Error(`npm ${args.join(" ")} exited with code ${result.status}`);
  }
}

function verifyNodeConsumer() {
  if (!checks.node) {
    throw new Error(`${packageName} has no node verification profile`);
  }
  const directory = join(consumerRoot, "node");
  mkdirSync(directory);
  writeJson(directory, "package.json", {
    name: "@concertable/package-verification",
    private: true,
    type: "module",
  });
  run(
    [
      "install",
      "--save-exact",
      installTarget,
      "react@19.1.0",
      "typescript@5.9",
      "@types/react@19",
    ],
    directory,
  );
  writeJson(directory, "tsconfig.json", {
    compilerOptions: {
      module: "NodeNext",
      moduleResolution: "NodeNext",
      target: "ES2022",
      strict: true,
      skipLibCheck: true,
      jsx: "react-jsx",
    },
    include: ["index.ts"],
  });
  writeFileSync(join(directory, "index.ts"), checks.node.join("\n") + "\n");
  // Runtime ESM smoke test — plain JS, so use an explicit runtime profile when the type-check
  // profile carries type-only syntax; otherwise the type-check lines are already valid JS.
  writeFileSync(join(directory, "index.mjs"), (checks.nodeRuntime ?? checks.node).join("\n") + "\n");
  run(["exec", "--", "tsc", "--noEmit"], directory);
  run(["exec", "--", "node", "index.mjs"], directory);
}

function verifyMetroConsumer() {
  if (!checks.metro) {
    throw new Error(`${packageName} has no metro verification profile`);
  }
  const directory = join(consumerRoot, "metro");
  mkdirSync(directory);
  writeJson(directory, "package.json", {
    name: "@concertable/metro-package-verification",
    private: true,
    main: "index.js",
  });
  run(
    [
      "install",
      "--save-exact",
      installTarget,
      "expo@54.0.33",
      "react@19.1.0",
      "react-native@0.81.5",
    ],
    directory,
  );
  writeJson(directory, "app.json", {
    expo: {
      name: "Concertable package verification",
      slug: "concertable-package-verification",
    },
  });
  writeFileSync(join(directory, "index.js"), checks.metro.join("\n") + "\n");
  run(
    ["exec", "--", "expo", "export", "--platform", "android", "--output-dir", "dist"],
    directory,
  );
}

try {
  if (verifyNode) {
    verifyNodeConsumer();
  }

  if (verifyMetro) {
    verifyMetroConsumer();
  }
} finally {
  rmSync(consumerRoot, { recursive: true, force: true });
}
