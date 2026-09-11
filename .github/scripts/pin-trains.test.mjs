import assert from 'node:assert/strict';
import { readFileSync, readdirSync } from 'node:fs';
import { join, relative } from 'node:path';
import { fileURLToPath } from 'node:url';
import test from 'node:test';

const repoRoot = fileURLToPath(new URL('../../', import.meta.url));
const apiRoot = join(repoRoot, 'api');
const inventoryPath = join(repoRoot, 'eng', 'repository-split', 'inventory.json');

const PLATFORM_PIN = 'ConcertableDotNetPlatformVersion';
const TARGET_PINS = new Map([
  ['platform-dotnet', PLATFORM_PIN],
  ['auth', 'ConcertableAuthVersion'],
  ['b2b', 'ConcertableB2BContractsVersion'],
  ['customer', 'ConcertableCustomerVersion'],
  ['payment', 'ConcertablePaymentVersion'],
  ['search', 'ConcertableSearchVersion'],
  ['system', 'ConcertableSystemVersion'],
]);
const TRAIN_PINS = new Set(TARGET_PINS.values());

function pinFiles() {
  const found = [];
  for (const entry of readdirSync(apiRoot, { withFileTypes: true, recursive: true })) {
    if (entry.isFile() && entry.name === 'Directory.Packages.props') {
      found.push(join(entry.parentPath ?? entry.path, entry.name));
    }
  }
  return found.sort();
}

function declaredProperties(xml) {
  const properties = new Map();
  for (const group of xml.matchAll(/<PropertyGroup[^>]*>([\s\S]*?)<\/PropertyGroup>/g)) {
    for (const entry of group[1].matchAll(/<(\w+)(?:\s[^>]*)?>([^<]*)<\/\1>/g)) {
      properties.set(entry[1], entry[2].trim());
    }
  }
  return properties;
}

function packageVersions(xml) {
  const refs = [];
  for (const match of xml.matchAll(/<PackageVersion\s+([^>]*?)\/?>/g)) {
    const attributes = match[1];
    const id = attributes.match(/Include="([^"]+)"/)?.[1];
    const version = attributes.match(/Version="([^"]+)"/)?.[1];
    if (id && version) refs.push({ id, version });
  }
  return refs;
}

function rootProperty(version, properties) {
  let current = version;
  for (let hop = 0; hop < 10; hop += 1) {
    const name = current.trim().match(/^\$\((\w+)\)$/)?.[1];
    if (!name) return null;
    if (!properties.has(name)) return name;
    const next = properties.get(name);
    if (!/^\$\(\w+\)$/.test(next.trim())) return name;
    current = next;
  }
  return null;
}

function inventoryTargets() {
  const inventory = JSON.parse(readFileSync(inventoryPath, 'utf8'));
  const targets = new Map();
  for (const project of Object.values(inventory.dotnet.projects)) {
    targets.set(project.name, project.target);
  }
  return targets;
}

test('every Concertable package id is pinned to the train that publishes it', () => {
  const targets = inventoryTargets();
  const failures = [];
  let checked = 0;

  for (const file of pinFiles()) {
    const xml = readFileSync(file, 'utf8');
    const properties = declaredProperties(xml);
    for (const { id, version } of packageVersions(xml)) {
      if (!id.startsWith('Concertable.')) continue;
      const target = targets.get(id);
      if (target === undefined) continue;
      checked += 1;
      const expected = TARGET_PINS.get(target);
      const actual = rootProperty(version, properties);
      if (expected === undefined || actual !== expected) {
        failures.push(
          `${relative(repoRoot, file)}: ${id} (inventory target "${target}") resolves through ` +
            `${actual ?? version} but must follow ${expected === undefined ? 'a known train' : `$(${expected})`}`,
        );
      }
    }
  }

  assert.ok(checked > 0, 'no Concertable package references found to check');
  assert.deepEqual(failures, [], `\n${failures.join('\n')}\n`);
});

test('every pin property resolves to a known train', () => {
  const orphans = [];
  for (const file of pinFiles()) {
    const xml = readFileSync(file, 'utf8');
    const properties = declaredProperties(xml);
    for (const [name, value] of properties) {
      if (!/^Concertable\w*Version$/.test(name)) continue;
      if (TRAIN_PINS.has(name)) continue;
      const root = rootProperty(`$(${name})`, properties);
      if (!TRAIN_PINS.has(root)) {
        orphans.push(`${relative(repoRoot, file)}: <${name}>${value}</${name}> resolves to ${root}`);
      }
    }
  }
  assert.deepEqual(orphans, [], `\n${orphans.join('\n')}\n`);
});

test('the local platform loop overrides every train for pack and consumption', () => {
  const script = readFileSync(join(repoRoot, 'scripts', 'local-platform.ps1'), 'utf8');
  const workflow = readFileSync(join(repoRoot, '.github', 'workflows', 'test.yml'), 'utf8');
  const missing = [...TRAIN_PINS].filter((name) => !script.includes(`-p:${name}=`));
  assert.deepEqual(missing, [], `scripts/local-platform.ps1 does not override: ${missing.join(', ')}`);
  assert.equal((script.match(/Get-LocalTrainArguments \$version/g) ?? []).length, 3);
  assert.equal((script.match(/\+ \$trainArguments/g) ?? []).length, 3);
  assert.match(script, /9999\.0\.0-local\.\$\(/);
  assert.match(workflow, /LOCAL_PLATFORM_VERSION: 9999\.0\.0-local\.\$\{\{/);
});
