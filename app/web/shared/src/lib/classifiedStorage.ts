import { consent } from "@/lib/consent";
import { STORAGE_MANIFEST } from "@/lib/storageManifest";

export interface ClassifiedStorage<T extends string = string> {
  get(): T | null;
  set(value: T): boolean;
  remove(): void;
}

export function createClassifiedStorage<T extends string = string>(
  key: string,
): ClassifiedStorage<T> {
  const item = STORAGE_MANIFEST.find(
    (entry) => entry.key === key && entry.firstParty,
  );
  if (item === undefined)
    throw new Error(
      `"${key}" has no first-party STORAGE_MANIFEST entry — classify it before storing it.`,
    );
  if (item.api !== "localStorage" && item.api !== "sessionStorage")
    throw new Error(
      `createClassifiedStorage handles web storage only; "${key}" is classified as ${item.api}.`,
    );

  const gate = item.consentCategory;
  const consentGated =
    item.classification === "analytics" || item.classification === "marketing";
  const store = () =>
    item.api === "localStorage" ? localStorage : sessionStorage;

  return {
    get: () => store().getItem(key) as T | null,
    set: (value) => {
      if (consentGated && (gate === undefined || !consent.has(gate)))
        return false;
      store().setItem(key, value);
      return true;
    },
    remove: () => store().removeItem(key),
  };
}
