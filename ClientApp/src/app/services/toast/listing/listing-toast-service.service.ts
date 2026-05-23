import { Injectable } from '@angular/core';
import { ToastService } from '../toast.service';

@Injectable({
  providedIn: 'root'
})
export class ListingToastServiceService extends ToastService {
  showApplied(name: string) {
    this.showSuccess(`Successfully applied to the listing ${name}`,"Applied")
  }
}
