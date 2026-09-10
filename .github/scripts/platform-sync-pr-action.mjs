import { pathToFileURL } from 'node:url';

export function classifySupersededPlatformSync({ mergeStateStatus, autoMergeRequest }) {
  const isAutoMergeArmed = autoMergeRequest !== null && autoMergeRequest !== undefined;
  return mergeStateStatus === 'CLEAN' && isAutoMergeArmed ? 'keep' : 'close';
}

async function main() {
  let input = '';
  process.stdin.setEncoding('utf8');

  for await (const chunk of process.stdin)
    input += chunk;

  process.stdout.write(classifySupersededPlatformSync(JSON.parse(input)));
}

if (process.argv[1] && import.meta.url === pathToFileURL(process.argv[1]).href)
  await main();
