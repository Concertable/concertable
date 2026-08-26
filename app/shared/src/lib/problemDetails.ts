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
): ApiErrorMessage | undefined {
  if (meta?.silenceErrors) return undefined;
  if (!isApiError(error)) return undefined;
  const { status } = error;
  if (status === 401) return undefined;
  if (status !== undefined && meta?.expectedErrors?.includes(status))
    return undefined;

  const { title, detail, errors } = error.details;
  if (errors?.length) return { title: title ?? "Error", errors };
  if (status === 404) return { message: detail ?? "Not found" };
  return { message: detail ?? "Something went wrong" };
}
