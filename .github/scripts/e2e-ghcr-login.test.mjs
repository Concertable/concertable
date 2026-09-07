import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import test from 'node:test';

const workflow = readFileSync(new URL('../workflows/test.yml', import.meta.url), 'utf8').replace(
  /\r\n?/g,
  '\n',
);

function jobBlock(name) {
  const marker = `\n  ${name}:\n`;
  const start = workflow.indexOf(marker);
  assert.notEqual(start, -1, `missing ${name} job`);
  const tail = workflow.slice(start + marker.length);
  const nextJob = tail.search(/^  [a-zA-Z0-9_-]+:\s*$/m);
  return nextJob === -1 ? tail : tail.slice(0, nextJob);
}

function escapeRegExp(value) {
  return value.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}

for (const [job, guard] of [
  ['e2e-api-tests', "needs.changes.outputs.run_e2e == 'true'"],
  ['e2e-ui-tests', "needs.changes.outputs.run_e2e_ui == 'true'"],
  ['e2e-ui-quarantine', "env.RUN == 'true'"],
]) {
  test(`${job} authenticates before pulling pinned service images`, () => {
    const block = jobBlock(job);
    assert.match(block, /permissions:\n      contents: read\n      packages: read/);
    assert.match(block, new RegExp(`- name: Log in to GHCR\\n        if: ${escapeRegExp(guard)}`));
    assert.match(block, /uses: docker\/login-action@v3/);
    assert.match(block, /registry: ghcr\.io/);
    assert.match(block, /username: \$\{\{ github\.actor \}\}/);
    assert.match(block, /password: \$\{\{ secrets\.GITHUB_TOKEN \}\}/);
  });
}
