import assert from 'node:assert/strict';
import test from 'node:test';
import { classifySupersededPlatformSync } from './platform-sync-pr-action.mjs';

const cases = [
  {
    name: 'keeps a clean PR with auto-merge armed',
    state: { mergeStateStatus: 'CLEAN', autoMergeRequest: { enabledAt: '2026-08-21T20:00:00Z' } },
    expected: 'keep',
  },
  {
    name: 'closes a clean PR without auto-merge armed',
    state: { mergeStateStatus: 'CLEAN', autoMergeRequest: null },
    expected: 'close',
  },
  {
    name: 'closes a blocked PR even when auto-merge is armed',
    state: { mergeStateStatus: 'BLOCKED', autoMergeRequest: { enabledAt: '2026-08-21T20:00:00Z' } },
    expected: 'close',
  },
  {
    name: 'closes a conflicting PR',
    state: { mergeStateStatus: 'DIRTY', autoMergeRequest: null },
    expected: 'close',
  },
];

for (const { name, state, expected } of cases) {
  test(name, () => {
    assert.equal(classifySupersededPlatformSync(state), expected);
  });
}
