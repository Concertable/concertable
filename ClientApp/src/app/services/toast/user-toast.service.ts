import { Injectable } from '@angular/core';
import { ToastService } from './toast.service';

@Injectable({
  providedIn: 'root'
})
export class UserToastService extends ToastService {

  showUpdated() {
    this.showInfo(`Your account has been updated`, "User Updated");
  }
}
