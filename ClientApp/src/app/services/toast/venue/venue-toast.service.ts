import { Injectable } from '@angular/core';
import { ToastService } from '../toast.service';

@Injectable({
  providedIn: 'root'
})
export class VenueToastService extends ToastService {
  showVenueCreated(name: string) {
    this.showSuccess(`Venue "${name}" has been successfully created!`, "Venue Created");
  }

  showVenueUpdated(name: string) {
    this.showInfo(`Venue "${name}" has been updated.`, "Venue Updated");
  }

  showVenueDeleted(name: string) {
    this.showWarning(`Venue "${name}" has been removed.`, "Venue Deleted");
  }
}
