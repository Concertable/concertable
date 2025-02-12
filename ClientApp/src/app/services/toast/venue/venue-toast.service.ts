import { Injectable } from '@angular/core';
import { ToastService } from '../toast.service';

@Injectable({
  providedIn: 'root'
})
export class VenueToastService extends ToastService {
  showCreated(name: string) {
    this.showSuccess(`Venue "${name}" has been successfully created!`, "Venue Created");
  }

  showUpdated(name: string) {
    this.showInfo(`Venue "${name}" has been updated.`, "Venue Updated");
  }

  showDeleted(name: string) {
    this.showWarning(`Venue "${name}" has been removed.`, "Venue Deleted");
  }
}
