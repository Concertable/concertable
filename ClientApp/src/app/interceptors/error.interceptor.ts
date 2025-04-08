import { Injectable } from "@angular/core";
import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { AuthService } from "../services/auth/auth.service";
import { Router } from "@angular/router";
import { Observable, catchError, throwError } from "rxjs";
import { ToastrService } from "ngx-toastr";
import { SKIP_ERROR_HANDLER } from "../shared/http/http-context.token";
import { extractHttpErrorMessage } from "../shared/http/http-interceptor-utils";
import { ToastService } from "../services/toast/toast.service";

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  
  constructor(private authService: AuthService, private router: Router, private toastService: ToastService) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (request.context.get(SKIP_ERROR_HANDLER)) 
      return next.handle(request);
    
    return next.handle(request).pipe(
      catchError((err: HttpErrorResponse) => {
        const { title, message } = extractHttpErrorMessage(err);
        this.toastService.showError(title, message);
        return throwError(() => err);
      })
    );
  }
}
