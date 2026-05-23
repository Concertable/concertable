import { Injectable } from '@angular/core';
import { ToastService } from './toast.service';

@Injectable({
  providedIn: 'root'
})
export class PreferenceToastService extends ToastService {
  showCreated() {
    this.showSuccess(`Preferences have been created!`, "Preferences Created");
  }

  showUpdated() {
    this.showInfo(`Your preferences have been updated.`, "Preferences Updated");
  }
}
