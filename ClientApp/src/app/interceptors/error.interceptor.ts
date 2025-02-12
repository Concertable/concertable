import { Injectable } from "@angular/core";
import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { AuthService } from "../services/auth/auth.service";
import { Router } from "@angular/router";
import { Observable, catchError, throwError } from "rxjs";
import { ToastrService } from "ngx-toastr";

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  
  constructor(private authService: AuthService, private router: Router, private toastr: ToastrService) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(request).pipe(
      catchError((err: HttpErrorResponse) => {
        console.error("HTTP Error:", err); // Logs the full error for debugging
        console.log(err.status)
        if (err.status === 400) {
          if (err.error.errors) {
            const modelStateErrors = [];
            for (const key in err.error.errors) {
              if (err.error.errors[key]) {
                modelStateErrors.push(err.error.errors[key]);
              }
            }
            this.toastr.error(modelStateErrors.flat().join(" "), "Validation Error");
          } else {
            this.toastr.error(err.error.title || err.error.message || "Bad Request", "Error 400");
          }
        }
        else if (err.status === 401) {
        }
        else if (err.status === 403) {
          
        }
        else if (err.status === 404) {
          this.router.navigateByUrl("/not-found");
        }
        else if (err.status === 500) {
          this.router.navigateByUrl("/server-error", { state: { error: err.error } });
        }
        else {
          this.router.navigateByUrl("/server-error", { state: { error: err.error } });
        }
        return throwError(() => err);
      })
    );
  }
}
