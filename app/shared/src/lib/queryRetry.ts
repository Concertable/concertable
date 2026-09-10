import { isApiError } from "./apiError";

const MAX_RETRIES = 2;
const TRANSIENT_STATUS = new Set([408, 429, 502, 503, 504]);

export function shouldRetry(failureCount: number, error: unknown): boolean {
  if (failureCount >= MAX_RETRIES) return false;

  if (isApiError(error)) {
    if (error.status === undefined) return true;
    return TRANSIENT_STATUS.has(error.status);
  }

  return false;
}
