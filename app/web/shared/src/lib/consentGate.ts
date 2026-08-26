import {
  hasConsent,
  onConsentChange,
  type ConsentCategory,
} from "@/lib/consent";

export interface ConsentGatedResource {
  category: ConsentCategory;
  activate: () => void;
  deactivate?: () => void;
}

/**
 * Runs `activate` only while `category` is granted and `deactivate` when it is
 * withdrawn, firing each once per edge. Applies the current decision immediately,
 * then tracks later changes. Returns a teardown that stops tracking (it does not
 * deactivate). This is the primitive that gives `hasConsent` teeth: non-essential
 * tech registers here and loads only after consent, never as a static import.
 */
export function registerConsentGated(resource: ConsentGatedResource): () => void {
  let active = false;

  const apply = (granted: boolean) => {
    if (granted === active) return;
    active = granted;
    if (granted) resource.activate();
    else resource.deactivate?.();
  };

  apply(hasConsent(resource.category));
  return onConsentChange((record) =>
    apply(hasConsent(resource.category, record)),
  );
}

export interface ConsentGatedScript {
  category: ConsentCategory;
  src: string;
  id?: string;
  attributes?: Record<string, string>;
}

/**
 * Injects a `<script>` into the document head once `category` is granted and
 * removes the tag when it is withdrawn. Tag removal stops the script re-loading;
 * it does not undo side effects already run — a granted-then-withdrawn tag that
 * set cookies is cleaned up by a reload, which the caller owns.
 */
export function registerConsentGatedScript(
  script: ConsentGatedScript,
): () => void {
  let element: HTMLScriptElement | null = null;

  return registerConsentGated({
    category: script.category,
    activate: () => {
      if (typeof document === "undefined" || element !== null) return;
      element = document.createElement("script");
      element.src = script.src;
      element.async = true;
      if (script.id !== undefined) element.id = script.id;
      for (const [name, value] of Object.entries(script.attributes ?? {})) {
        element.setAttribute(name, value);
      }
      document.head.appendChild(element);
    },
    deactivate: () => {
      element?.remove();
      element = null;
    },
  });
}
