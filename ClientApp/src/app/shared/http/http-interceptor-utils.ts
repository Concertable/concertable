import { HttpErrorResponse } from "@angular/common/http";

export function extractHttpErrorMessage(error: HttpErrorResponse): { title: string, message: string } {
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
          message: modelStateErrors.flat().join(" ")
        };
      }
    }
  
    // Non-400 errors
    console.log("erry",error);
    return {
      title: `${error.status} ${error.statusText}` || "Error",
      message: error.error?.message || error.message || "An unexpected error occurred"
    };
  }
  