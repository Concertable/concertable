import { isApiError, type ProblemDetails } from "./apiError";

export type { ProblemDetails } from "./apiError";

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

export type ApiErrorMessage =
  | { title: string; errors: string[] }
  | { message: string };

export function resolveApiError(
  error: unknown,
  meta: ErrorMeta | undefined,
): ApiErrorMessage | null {
  if (meta?.silenceErrors) return null;
  if (!isApiError(error)) return null;
  const { status } = error;
  if (status === 401) return null;
  if (status !== null && meta?.expectedErrors?.includes(status)) return null;

  const { title, detail, errors } = error.details;
  if (errors?.length) return { title: title ?? "Error", errors };
  if (status === 404) return { message: detail ?? "Not found" };
  return { message: detail ?? "Something went wrong" };
}
