import { hasConsent } from "@/lib/consent";
import { STORAGE_MANIFEST } from "@/lib/storageManifest";

/**
 * A typed handle to one classified first-party web-storage item — the single sanctioned
 * way our own code reads or writes {@link Storage}. It derives its classification and
 * consent gate from the item's {@link STORAGE_MANIFEST} entry, so an `analytics`/`marketing`
 * item cannot be written before its consent category is granted, and no item can be stored
 * at all until it is classified. This is the storage-write counterpart to
 * `registerConsentGated` (which gates resource/script activation).
 */
export interface ClassifiedStorage<T extends string = string> {
  get(): T | null;
  /** Stores the value unless a consent gate withholds it; returns whether it was written. */
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
  const store = () => (item.api === "localStorage" ? localStorage : sessionStorage);

  return {
    get: () => store().getItem(key) as T | null,
    set: (value) => {
      if (consentGated && (gate === undefined || !hasConsent(gate))) return false;
      if (item.api === "localStorage") localStorage.setItem(key, value);
      else sessionStorage.setItem(key, value);
      return true;
    },
    remove: () => store().removeItem(key),
  };
}
