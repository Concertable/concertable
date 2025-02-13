import { Injectable } from '@angular/core';
import { ToastService } from '../toast.service';

@Injectable({
  providedIn: 'root'
})
export class EventToastService extends ToastService {
  showAutoCreated() {
    this.showSuccess("Successfully created the event","Created")
  }
}
