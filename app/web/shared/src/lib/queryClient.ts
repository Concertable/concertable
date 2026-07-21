import { QueryClient, QueryCache, MutationCache } from "@tanstack/react-query";
import { isAxiosError } from "axios";
import { toast } from "sonner";
import { createElement } from "react";
import { shouldRetry } from "@concertable/shared/lib/queryRetry";
import { resolveApiError, type ErrorMeta } from "@concertable/shared/lib/problemDetails";

function handleError(error: unknown, meta: ErrorMeta | undefined) {
  const resolved = resolveApiError(error, meta);
  if (!resolved) return;

  if (import.meta.env.DEV && isAxiosError(error))
    console.warn("[toast]", error.response?.status, error.config?.url);

  if ("errors" in resolved) {
    toast.error(resolved.title, {
      description: createElement(
        "ul",
        { className: "list-disc space-y-1 pl-4" },
        resolved.errors.map((e, i) => createElement("li", { key: i }, e)),
      ),
    });
  } else {
    toast.error(resolved.message);
  }
}

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 60 * 1000 * 5,
      refetchOnWindowFocus: false,
      retry: shouldRetry,
    },
  },
  queryCache: new QueryCache({
    onError: (error, query) => handleError(error, query.meta as ErrorMeta | undefined),
  }),
  mutationCache: new MutationCache({
    onError: (error, _vars, _ctx, mutation) =>
      handleError(error, mutation.meta as ErrorMeta | undefined),
  }),
});
