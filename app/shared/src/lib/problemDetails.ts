import { isAxiosError, type AxiosError } from "axios";

export type ProblemDetails = {
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

export type ApiErrorMessage =
  | { title: string; errors: string[] }
  | { message: string };

// The platform-agnostic half of central error handling: classify a query/mutation error into what
// to surface, or null to swallow it (silenced, non-axios, 401, or a status the caller expects). Each
// platform renders the result with its own toast (sonner on web, react-native-toast-message on mobile).
export function resolveApiError(
  error: unknown,
  meta: ErrorMeta | undefined,
): ApiErrorMessage | null {
  if (meta?.silenceErrors) return null;
  if (!isAxiosError(error)) return null;
  const axiosError = error as AxiosError<ProblemDetails>;
  const status = axiosError.response?.status ?? 0;
  if (status === 401) return null;
  if (meta?.expectedErrors?.includes(status)) return null;

  const { title, detail, errors } = axiosError.response?.data ?? {};
  if (errors?.length) return { title: title ?? "Error", errors };
  if (status === 404) return { message: detail ?? "Not found" };
  return { message: detail ?? "Something went wrong" };
}
