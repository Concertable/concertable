import {HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { AuthService } from "../services/auth/auth.service";
import { Observable, catchError, switchMap, throwError } from "rxjs";
import { Injectable } from "@angular/core";

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

  constructor(private authService: AuthService) { }

  intercept(request: HttpRequest<any>, next: HttpHandler) : Observable<HttpEvent<any>> {
    return next.handle(request).pipe(
      catchError((err: HttpErrorResponse) => {
        console.log("err",err)
        if (err.status === 400) {
          if (err.error.errors) {
            const modelStateErrors = [];
            for (const key in err.error.errors) {
              if (err.error.errors[key]) {
                modelStateErrors.push(err.error.errors[key])
              }
            }
            throw modelStateErrors.flat();
          } else {
            //snackbar.error(err.error.title || err.error);
          }
        }
        if (err.status === 401) {
          //snackbar.error(err.error.title || err.error);
        }
        if (err.status === 403) {
          //snackbar.error('Forbidden');
        }
        if (err.status === 404) {
          //router.navigateByUrl('/not-found');
        }
        if (err.status === 500) {
          //const navigationExtras: NavigationExtras = {state: {error: err.error}}
          //router.navigateByUrl('/server-error', navigationExtras);
        }
        return throwError(() => err)
      })
    );
  }
}