import { HttpErrorResponse } from "@angular/common/http";

export function extractHttpErrorMessage(error: HttpErrorResponse): { title: string, status?: number, message?: string } {
    console.log(error)
    // Handle ModelState errors 400
    if (error.status === 400) {
      if (error.error?.errors) {
        const modelStateErrors = [];
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
    }
  
      // Handle raw string error response 
    if (typeof error.error === 'string') {
      return {
        title: `${error.status} ${error.statusText}`,
        message: error.error 
      };
    }

    // ProblemDetails-like JSON error
    if (typeof error.error === 'object') {
      return {
        title: error.error.title || `${error.status} ${error.statusText}`,
        message: error.error.message
      };
    }

    // Fallback
    return {
      title: `${error.status} ${error.statusText}`,
      message: error.message 
    };
  }
  