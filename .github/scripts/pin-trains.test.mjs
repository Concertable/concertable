import assert from 'node:assert/strict';
import { readFileSync, readdirSync } from 'node:fs';
import { join, relative } from 'node:path';
import { fileURLToPath } from 'node:url';
import test from 'node:test';

const repoRoot = fileURLToPath(new URL('../../', import.meta.url));
const apiRoot = join(repoRoot, 'api');
const inventoryPath = join(repoRoot, 'eng', 'repository-split', 'inventory.json');

const PLATFORM_PIN = 'ConcertableDotNetPlatformVersion';
const SERVICE_PIN = 'ConcertableServiceVersion';
const PLATFORM_TARGET = 'platform-dotnet';

// Two trains publish Concertable.* packages and they advance independently. An id must be pinned to
// the train that actually publishes it: ask for a version on the wrong train and restore fails
// NU1102 on an id that never shipped that version. A pin property may resolve through OTHER pin
// properties (ConcertablePaymentVersion defaults to one of the two), so the train an id follows is
// the ROOT of its property chain, never the attribute written next to it.
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

// The root of the property chain, so a pin that defaults to another pin is attributed to the train
// it actually lands on rather than to the intermediate property.
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
      const expected = target === PLATFORM_TARGET ? PLATFORM_PIN : SERVICE_PIN;
      const actual = rootProperty(version, properties);
      if (actual !== expected) {
        failures.push(
          `${relative(repoRoot, file)}: ${id} (inventory target "${target}") resolves through ` +
            `${actual ?? version} but must follow $(${expected})`,
        );
      }
    }
  }

  assert.ok(checked > 0, 'no Concertable package references found to check');
  assert.deepEqual(failures, [], `\n${failures.join('\n')}\n`);
});

test('an id whose inventory target is a service is never on the platform train', () => {
  const targets = inventoryTargets();
  const onPlatform = [];
  for (const file of pinFiles()) {
    const xml = readFileSync(file, 'utf8');
    const properties = declaredProperties(xml);
    for (const { id, version } of packageVersions(xml)) {
      if (!id.startsWith('Concertable.')) continue;
      if (rootProperty(version, properties) !== PLATFORM_PIN) continue;
      const target = targets.get(id);
      if (target !== undefined && target !== PLATFORM_TARGET) {
        onPlatform.push(`${relative(repoRoot, file)}: ${id} -> target "${target}"`);
      }
    }
  }
  assert.deepEqual(onPlatform, [], `\n${onPlatform.join('\n')}\n`);
});

// The pin files are the only place the two property names are allowed to originate. A third pin
// appearing without a train behind it is the shape that broke local-platform-pack.
test('every pin property resolves to one of the two trains', () => {
  const orphans = [];
  for (const file of pinFiles()) {
    const xml = readFileSync(file, 'utf8');
    const properties = declaredProperties(xml);
    for (const [name, value] of properties) {
      if (!/^Concertable\w*Version$/.test(name)) continue;
      if (name === PLATFORM_PIN || name === SERVICE_PIN) continue;
      const root = rootProperty(`$(${name})`, properties);
      if (root !== PLATFORM_PIN && root !== SERVICE_PIN) {
        orphans.push(`${relative(repoRoot, file)}: <${name}>${value}</${name}> resolves to ${root}`);
      }
    }
  }
  assert.deepEqual(orphans, [], `\n${orphans.join('\n')}\n`);
});
