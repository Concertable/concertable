import assert from 'node:assert/strict';
import { execFileSync } from 'node:child_process';
import { cpSync, mkdirSync, mkdtempSync, readFileSync, readdirSync, rmSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join, relative } from 'node:path';
import { fileURLToPath } from 'node:url';
import test from 'node:test';

const script = fileURLToPath(new URL('./bump-platform-version.sh', import.meta.url));
const repoRoot = fileURLToPath(new URL('../../', import.meta.url));
const apiRoot = join(repoRoot, 'api');

// The pin element the script keys off, read from the script rather than restated, so this test
// cannot pass by agreeing with itself.
function scriptPinElement() {
  const source = readFileSync(script, 'utf8');
  const match = source.match(/grep -rlE '<([A-Za-z]+)>\[\^<\]\+<\/\1>'/);
  assert.notEqual(match, null, 'bump-platform-version.sh no longer discovers pin files by element name');
  return match[1];
}

function pinFiles(root) {
  const found = [];
  for (const entry of readdirSync(root, { withFileTypes: true, recursive: true })) {
    if (entry.isFile() && entry.name === 'Directory.Packages.props') {
      found.push(join(entry.parentPath ?? entry.path, entry.name));
    }
  }
  return found.sort();
}

function declaredElements(file) {
  const content = readFileSync(file, 'utf8');
  return [...content.matchAll(/<(Concertable[A-Za-z]*PlatformVersion)>[^<]+<\/\1>/g)].map((m) => m[1]);
}

function stageTree(pins) {
  const root = mkdtempSync(join(tmpdir(), 'bump-pin-'));
  mkdirSync(join(root, '.github', 'scripts'), { recursive: true });
  cpSync(script, join(root, '.github', 'scripts', 'bump-platform-version.sh'));
  for (const [path, content] of Object.entries(pins)) {
    const target = join(root, path);
    mkdirSync(join(target, '..'), { recursive: true });
    writeFileSync(target, content, 'utf8');
  }
  mkdirSync(join(root, 'api'), { recursive: true });
  return root;
}

// The script resolves and prints its own absolute paths, which on Windows arrive in the shell's
// own path style rather than the caller's — so compare the repository-relative tail, not the root.
function bump(root, version) {
  return execFileSync('bash', [join(root, '.github', 'scripts', 'bump-platform-version.sh'), version], {
    encoding: 'utf8',
  })
    .split('\n')
    .filter((line) => line.trim())
    .map((line) => line.trim().split(/[\\/]/).slice(-3).join('/'))
    .sort();
}

function pinFile(element, version) {
  return `<Project>\n  <PropertyGroup>\n    <${element}>${version}</${element}>\n  </PropertyGroup>\n  <ItemGroup>\n    <PackageVersion Include="Concertable.Kernel" Version="$(${element})" />\n  </ItemGroup>\n</Project>\n`;
}

// The rename trap: the element lives in seven pin files and in this script's discovery regex,
// refusal guard and in-place sed. Rename the property and miss the script and platform-sync
// silently stops bumping anything, leaving every service pinned to a stale platform forever.
test('the script discovers the element the pin files actually declare', () => {
  const element = scriptPinElement();
  const declaring = pinFiles(apiRoot).filter((file) => declaredElements(file).length > 0);
  assert.ok(declaring.length > 0, 'no api/**/Directory.Packages.props declares a platform pin');
  for (const file of declaring) {
    assert.deepEqual(
      declaredElements(file),
      [element],
      `${relative(repoRoot, file)} declares a pin the bump script does not discover`,
    );
  }
});

test('every declaring file is bumped, and only those', () => {
  const element = scriptPinElement();
  const root = stageTree({
    'api/One/Directory.Packages.props': pinFile(element, '0.1.0-alpha.0.1'),
    'api/Two/Directory.Packages.props': pinFile(element, '0.1.0-alpha.0.2'),
    'api/NoPin/Directory.Packages.props': '<Project>\n  <PropertyGroup />\n</Project>\n',
  });
  try {
    assert.deepEqual(bump(root, '0.1.0-alpha.0.9'), [
      'api/One/Directory.Packages.props',
      'api/Two/Directory.Packages.props',
    ]);
    for (const name of ['One', 'Two']) {
      const content = readFileSync(join(root, 'api', name, 'Directory.Packages.props'), 'utf8');
      assert.match(content, new RegExp(`<${element}>0\\.1\\.0-alpha\\.0\\.9</${element}>`));
    }
  } finally {
    rmSync(root, { recursive: true, force: true });
  }
});

test('a file already at the target is left untouched and not printed', () => {
  const element = scriptPinElement();
  const root = stageTree({ 'api/One/Directory.Packages.props': pinFile(element, '0.1.0-alpha.0.9') });
  try {
    assert.deepEqual(bump(root, '0.1.0-alpha.0.9'), []);
  } finally {
    rmSync(root, { recursive: true, force: true });
  }
});

test('a tree with no pins is refused rather than silently reported as synced', () => {
  const root = stageTree({});
  try {
    assert.throws(() => bump(root, '0.1.0-alpha.0.9'), (error) => {
      assert.equal(error.status, 1);
      assert.match(error.stderr, /pins found under api\/ — refusing/);
      return true;
    });
  } finally {
    rmSync(root, { recursive: true, force: true });
  }
});
