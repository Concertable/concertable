import { Injectable } from '@angular/core';
import { ToastService } from '../toast.service';

@Injectable({
  providedIn: 'root'
})
export class ArtistToastService extends ToastService {
  showCreated(name: string) {
    this.showSuccess(`Artist "${name}" has been successfully created!`, "Artist Created");
  }

  showUpdated(name: string) {
    this.showInfo(`Artist "${name}" has been updated.`, "Artist Updated");
  }

  showDeleted(name: string) {
    this.showWarning(`Artist "${name}" has been removed.`, "Artist Deleted");
  }
}
