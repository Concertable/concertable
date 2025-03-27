import { Injectable } from '@angular/core';
import { ToastService } from '../toast.service';

@Injectable({
  providedIn: 'root'
})
export class EventToastService extends ToastService {
  showUpdated() {
    this.showSuccess("Successfully updated the event","Updated")
  }
  showAutoCreated() {
    this.showSuccess("Successfully created the event","Created")
  }
}
