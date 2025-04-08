import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ToastrService } from 'ngx-toastr';

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  constructor(protected toastr: ToastrService) {}

  showSuccess(message: string, title: string = "Success") {
    this.toastr.success(message, title);
  }

  showError(message: string, title: string = "Error") {
    this.toastr.error(message, title);
  }

  showErrorResponse(error: HttpErrorResponse) {
    this.toastr.error(error.error, `${error.status} ${error.statusText}`);
  }

  showWarning(message: string, title: string = "Warning") {
    this.toastr.warning(message, title);
  }

  showInfo(message: string, title: string = "Info") {
    this.toastr.info(message, title);
  }
}
