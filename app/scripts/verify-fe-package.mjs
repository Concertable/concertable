import { mkdirSync, mkdtempSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { dirname, join } from "node:path";
import { spawnSync } from "node:child_process";

const packageSpec = process.argv[2];
const verifyMetro = process.argv.includes("--metro");

if (!packageSpec) {
  throw new Error("Usage: node verify-fe-package.mjs <package-spec> [--metro]");
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
      packageSpec,
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
    },
    include: ["index.ts"],
  });
  writeFileSync(
    join(directory, "index.ts"),
    [
      'import { genreLabel } from "@concertable/shared";',
      'import { useMountEffect } from "@concertable/shared/hooks/useMountEffect";',
      'import type { Genre } from "@concertable/shared/types";',
      'import { useAuthStore, type User } from "@concertable/shared/features/auth";',
      'const genre: Genre = "Rock";',
      "if (genreLabel(genre) !== genre) throw new Error(\"Unexpected genre label\");",
      'if (typeof useMountEffect !== "function") throw new Error("Missing useMountEffect export");',
      'if (typeof useAuthStore !== "function") throw new Error("Missing useAuthStore export");',
      "const user = {} as User;",
      "void user;",
      "",
    ].join("\n"),
  );
  writeFileSync(
    join(directory, "index.mjs"),
    [
      'import { genreLabel } from "@concertable/shared";',
      'import { useMountEffect } from "@concertable/shared/hooks/useMountEffect";',
      'import { useAuthStore } from "@concertable/shared/features/auth";',
      'if (genreLabel("Rock") !== "Rock") throw new Error("Unexpected genre label");',
      'if (typeof useMountEffect !== "function") throw new Error("Missing useMountEffect export");',
      'if (typeof useAuthStore !== "function") throw new Error("Missing useAuthStore export");',
      "",
    ].join("\n"),
  );
  run(["exec", "--", "tsc", "--noEmit"], directory);
  run(["exec", "--", "node", "index.mjs"], directory);
}

function verifyMetroConsumer() {
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
      packageSpec,
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
  writeFileSync(
    join(directory, "index.js"),
    [
      'import { registerRootComponent } from "expo";',
      'import React from "react";',
      'import { Text } from "react-native";',
      'import { useAuthStore } from "@concertable/shared/features/auth";',
      'import { useMountEffect } from "@concertable/shared/hooks/useMountEffect";',
      'import { genreLabel } from "@concertable/shared/types";',
      "function App() {",
      "  useMountEffect(() => undefined);",
      "  void useAuthStore;",
      '  return React.createElement(Text, null, genreLabel("Rock"));',
      "}",
      "registerRootComponent(App);",
      "",
    ].join("\n"),
  );
  run(
    ["exec", "--", "expo", "export", "--platform", "android", "--output-dir", "dist"],
    directory,
  );
}

try {
  verifyNodeConsumer();

  if (verifyMetro) {
    verifyMetroConsumer();
  }
} finally {
  rmSync(consumerRoot, { recursive: true, force: true });
}
