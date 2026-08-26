import { spawnSync } from "node:child_process";
import { readFileSync, readdirSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";
import ts from "typescript";

const appRoot = dirname(dirname(fileURLToPath(import.meta.url)));
const executable = join(appRoot, "node_modules", "dependency-cruiser", "bin", "dependency-cruise.mjs");
const workspaces = [
  ["web/customer", "web/customer/tsconfig.app.json"],
  ["web/b2b/venue", "web/b2b/venue/tsconfig.app.json"],
  ["web/b2b/artist", "web/b2b/artist/tsconfig.app.json"],
  ["web/b2b/business", "web/b2b/business/tsconfig.app.json"],
  ["web/admin", "web/admin/tsconfig.app.json"],
  ["mobile/customer", "mobile/customer/tsconfig.json"],
  ["mobile/b2b", "mobile/b2b/tsconfig.json"],
  ["b2b/shared", "b2b/shared/tsconfig.build.json"],
  ["shared", "shared/tsconfig.build.json"],
  ["web/shared", "web/shared/tsconfig.build.json"],
  ["web/b2b/shared", "web/b2b/shared/tsconfig.build.json"],
  ["mobile/shared", "mobile/shared/tsconfig.build.json"],
  ["customer/shared", "customer/shared/tsconfig.build.json"],
];

const bareFeatureEntryPoint = /^@concertable\/[^/]+\/features\/[^/]+$/;
const featureTypesEntryPoint = /^@concertable\/[^/]+\/features\/[^/]+\/types$/;

function* sourceFiles(directory) {
  for (const entry of readdirSync(directory, { withFileTypes: true })) {
    const path = join(directory, entry.name);
    if (entry.isDirectory()) {
      yield* sourceFiles(path);
    } else if (/\.tsx?$/.test(entry.name) && !entry.name.endsWith(".d.ts")) {
      yield path;
    }
  }
}

function checkFeatureEntryPoints() {
  let violations = 0;

  for (const [workspace] of workspaces) {
    for (const path of sourceFiles(join(appRoot, workspace))) {
      const sourceFile = ts.createSourceFile(
        path,
        readFileSync(path, "utf8"),
        ts.ScriptTarget.Latest,
        true,
        path.endsWith(".tsx") ? ts.ScriptKind.TSX : ts.ScriptKind.TS,
      );

      for (const statement of sourceFile.statements) {
        const moduleSpecifier = statement.moduleSpecifier;
        if (!moduleSpecifier || !ts.isStringLiteral(moduleSpecifier)) {
          continue;
        }

        const hasTypeImport = ts.isImportDeclaration(statement)
          && (statement.importClause?.isTypeOnly
            || statement.importClause?.namedBindings && ts.isNamedImports(statement.importClause.namedBindings)
              && statement.importClause.namedBindings.elements.some((element) => element.isTypeOnly));
        const hasTypeExport = ts.isExportDeclaration(statement)
          && (statement.isTypeOnly
            || statement.exportClause && ts.isNamedExports(statement.exportClause)
              && statement.exportClause.elements.some((element) => element.isTypeOnly));
        const hasValueImport = ts.isImportDeclaration(statement)
          && statement.importClause
          && !statement.importClause.isTypeOnly
          && (statement.importClause.name
            || statement.importClause.namedBindings && ts.isNamespaceImport(statement.importClause.namedBindings)
            || statement.importClause.namedBindings && ts.isNamedImports(statement.importClause.namedBindings)
              && statement.importClause.namedBindings.elements.some((element) => !element.isTypeOnly));
        const hasValueExport = ts.isExportDeclaration(statement)
          && !statement.isTypeOnly
          && statement.exportClause
          && ts.isNamedExports(statement.exportClause)
          && statement.exportClause.elements.some((element) => !element.isTypeOnly);

        let rule;
        if (bareFeatureEntryPoint.test(moduleSpecifier.text) && (hasTypeImport || hasTypeExport)) {
          rule = "feature-type-import-requires-types-entrypoint";
        } else if (featureTypesEntryPoint.test(moduleSpecifier.text) && (hasValueImport || hasValueExport)) {
          rule = "feature-runtime-import-requires-feature-entrypoint";
        } else {
          continue;
        }

        const { line, character } = sourceFile.getLineAndCharacterOfPosition(statement.getStart(sourceFile));
        process.stderr.write(`${path}:${line + 1}:${character + 1} ${rule}: ${moduleSpecifier.text}\n`);
        violations += 1;
      }
    }
  }

  return violations;
}

let failed = checkFeatureEntryPoints() > 0;
for (const [workspace, tsConfig] of workspaces) {
  const result = spawnSync(
    process.execPath,
    [executable, workspace, "--config", ".dependency-cruiser.cjs", "--ts-config", join(appRoot, tsConfig), "--output-type", "err"],
    { cwd: appRoot, encoding: "utf8" },
  );
  process.stdout.write(result.stdout ?? "");
  process.stderr.write(result.stderr ?? "");
  failed ||= result.status !== 0;
}

process.exitCode = failed ? 1 : 0;
