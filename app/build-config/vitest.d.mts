export function nodeTests(rootDirectory: string): {
  resolve: { alias: { find: string; replacement: string }[] };
  test: { environment: "node"; include: string[] };
};
