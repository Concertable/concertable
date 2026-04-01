import queryString from "query-string";

export function stringifySearch(search: Record<string, unknown>): string {
  const str = queryString.stringify(search, { arrayFormat: "comma", skipNull: true, sort: false });
  return str ? `?${str}` : "";
}

export function parseSearch(searchStr: string): Record<string, unknown> {
  return queryString.parse(searchStr, { arrayFormat: "comma", parseNumbers: true, parseBooleans: true }) as Record<string, unknown>;
}
