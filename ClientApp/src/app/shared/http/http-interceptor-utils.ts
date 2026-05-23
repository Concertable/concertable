import { HttpErrorResponse } from "@angular/common/http";

export function extractHttpErrorMessage(error: HttpErrorResponse): { title: string, status?: number, message?: string } {
  console.log(error);

  // Handle ModelState errors 400
  if (error.status === 400 && error.error?.errors) {
    const modelStateErrors: string[] = [];
    for (const key in error.error.errors) {
      if (error.error.errors[key]) {
        modelStateErrors.push(error.error.errors[key]);
      }
    }
    return {
      title: "Validation Error",
      message: modelStateErrors.flat().join("<br>")
    };
  }

  // Handle string error response
  if (typeof error.error === 'string') {
    return {
      title: `${error.status} ${error.statusText}`,
      message: error.error
    };
  }

  // Handle ProblemDetails-style error with detail/message
  if (typeof error.error === 'object') {
    const errObj = error.error;
    return {
      title: errObj.title || `${error.status} ${error.statusText}`,
      message: errObj.detail || errObj.message || error.message
    };
  }

  // Fallback
  return {
    title: `${error.status} ${error.statusText}`,
    message: error.message
  };
}
