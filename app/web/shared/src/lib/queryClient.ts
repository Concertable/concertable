import { QueryClient, QueryCache, MutationCache } from "@tanstack/react-query";
import { isAxiosError, type AxiosError } from "axios";
import { toast } from "sonner";
import { createElement } from "react";
import { shouldRetry } from "@concertable/shared/lib/queryRetry";

type ProblemDetails = {
  title?: string;
  detail?: string;
  errors?: string[];
};

export type ErrorMeta = {
  silenceErrors?: boolean;
  expectedErrors?: number[];
};

declare module "@tanstack/react-query" {
  interface Register {
    queryMeta: ErrorMeta;
    mutationMeta: ErrorMeta;
  }
}

function handleError(error: unknown, meta: ErrorMeta | undefined) {
  if (meta?.silenceErrors) return;
  if (!isAxiosError(error)) return;
  const axiosError = error as AxiosError<ProblemDetails>;
  const status = axiosError.response?.status ?? 0;
  if (status === 401) return;
  if (meta?.expectedErrors?.includes(status)) return;

  if (import.meta.env.DEV)
    console.warn("[toast]", status, axiosError.config?.url);

  const { title, detail, errors } = axiosError.response?.data ?? {};
  if (errors?.length) {
    toast.error(title ?? "Error", {
      description: createElement(
        "ul",
        { className: "list-disc space-y-1 pl-4" },
        errors.map((e, i) => createElement("li", { key: i }, e)),
      ),
    });
  } else if (status === 404) {
    toast.error(detail ?? "Not found");
  } else {
    toast.error(detail ?? "Something went wrong");
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
