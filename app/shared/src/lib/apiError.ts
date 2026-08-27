export type ProblemDetails = {
  title?: string;
  detail?: string;
  errors?: string[];
};

export class ApiError extends Error {
  readonly name = "ApiError";

  constructor(
    readonly status: number | undefined,
    readonly details: ProblemDetails,
    readonly method: string | undefined,
    readonly url: string | undefined,
    readonly cause: unknown,
  ) {
    super(
      details.detail ??
        details.title ??
        (status === undefined
          ? "The request could not be completed"
          : `The request failed with status ${status}`),
    );
  }
}

export function isApiError(error: unknown): error is ApiError {
  return error instanceof ApiError;
}
