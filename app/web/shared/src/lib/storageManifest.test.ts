import { readdirSync, readFileSync } from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { describe, expect, it } from "vitest";
import { STORAGE_MANIFEST, type StorageApi } from "./storageManifest";

const APP_WEB_DIR = path.resolve(
  path.dirname(fileURLToPath(import.meta.url)),
  "../../..",
);

// The single sanctioned first-party write location: raw storage writes here are
// createClassifiedStorage itself. Every other first-party write must route through the
// accessor or be a declared library/substrate write site — so a new raw write elsewhere
// fails the guard until it is classified, rather than being detected only after the fact.
const ACCESSOR_MODULE = "shared/src/lib/classifiedStorage.ts";

const SKIP_DIRS = new Set([
  "node_modules",
  "dist",
  ".git",
  ".turbo",
  "coverage",
  "build",
]);

const WRITE_PATTERNS: { api: StorageApi; regex: RegExp }[] = [
  {
    api: "localStorage",
    regex: /(?:window\s*\.\s*)?\blocalStorage\s*\.\s*setItem\s*\(/g,
  },
  {
    api: "localStorage",
    regex: /(?:window\s*\.\s*)?\blocalStorage\s*\[[^\]\n]+\]\s*=(?!=)/g,
  },
  {
    api: "sessionStorage",
    regex: /(?:window\s*\.\s*)?\bsessionStorage\s*\.\s*setItem\s*\(/g,
  },
  {
    api: "sessionStorage",
    regex: /(?:window\s*\.\s*)?\bsessionStorage\s*\[[^\]\n]+\]\s*=(?!=)/g,
  },
  { api: "cookie", regex: /\bdocument\s*\.\s*cookie\s*=(?!=)/g },
  { api: "indexedDB", regex: /\bindexedDB\s*\.\s*open\s*\(/g },
  // zustand persist() middleware writes localStorage by default — no explicit setItem call.
  { api: "localStorage", regex: /\bpersist\s*\(/g },
];

function sourceFiles(dir: string): string[] {
  const files: string[] = [];
  for (const entry of readdirSync(dir, { withFileTypes: true })) {
    if (entry.isDirectory()) {
      if (!SKIP_DIRS.has(entry.name)) {
        files.push(...sourceFiles(path.join(dir, entry.name)));
      }
      continue;
    }
    const name = entry.name;
    if (/\.test\.tsx?$/.test(name)) continue;
    if (/\.(ts|tsx)$/.test(name)) files.push(path.join(dir, entry.name));
  }
  return files;
}

function scannedWrites(): string[] {
  const tokens: string[] = [];
  for (const file of sourceFiles(APP_WEB_DIR)) {
    const content = readFileSync(file, "utf8");
    const relative = path
      .relative(APP_WEB_DIR, file)
      .split(path.sep)
      .join("/");
    for (const { api, regex } of WRITE_PATTERNS) {
      const count = content.match(regex)?.length ?? 0;
      for (let i = 0; i < count; i++) tokens.push(`${relative}|${api}`);
    }
  }
  return tokens;
}

function declaredWrites(): string[] {
  return STORAGE_MANIFEST.filter((item) => item.firstParty).flatMap((item) =>
    (item.writeSites ?? []).map((site) => `${site}|${item.api}`),
  );
}

function multisetDiff(a: string[], b: string[]): string[] {
  const remaining = [...b];
  const extra: string[] = [];
  for (const token of a) {
    const index = remaining.indexOf(token);
    if (index === -1) extra.push(token);
    else remaining.splice(index, 1);
  }
  return extra;
}

describe("storage manifest drift guard", () => {
  const scanned = scannedWrites();
  const accessorWrites = scanned.filter((token) =>
    token.startsWith(`${ACCESSOR_MODULE}|`),
  );
  const directWrites = scanned.filter(
    (token) => !token.startsWith(`${ACCESSOR_MODULE}|`),
  );

  it("routes first-party storage through the classified accessor", () => {
    expect(
      accessorWrites.length,
      `${ACCESSOR_MODULE} performs no storage write — the classified accessor must be the first-party write path.`,
    ).toBeGreaterThan(0);
  });

  it("every direct storage write is a declared library/substrate site", () => {
    const undeclared = multisetDiff(directWrites, declaredWrites());
    expect(
      undeclared,
      `Unclassified direct storage write(s). Route first-party writes through createClassifiedStorage, or — for a library/substrate write the accessor cannot mediate — add it to STORAGE_MANIFEST in app/web/shared/src/lib/storageManifest.ts with its owner/purpose/duration/classification and a writeSites entry (file|api):\n${undeclared.join("\n")}`,
    ).toEqual([]);
  });

  it("every declared write site still writes storage", () => {
    const stale = multisetDiff(declaredWrites(), directWrites);
    expect(
      stale,
      `Manifest declares write sites that no longer write storage — remove or fix them in storageManifest.ts:\n${stale.join("\n")}`,
    ).toEqual([]);
  });

  it("analytics/marketing items name the consent category that gates them", () => {
    const misfiled = STORAGE_MANIFEST.filter(
      (item) =>
        (item.classification === "analytics" ||
          item.classification === "marketing") &&
        item.consentCategory === undefined,
    ).map((item) => item.key);
    expect(
      misfiled,
      `Consent-requiring items must set consentCategory so the gate knows what to withhold:\n${misfiled.join("\n")}`,
    ).toEqual([]);
  });
});
