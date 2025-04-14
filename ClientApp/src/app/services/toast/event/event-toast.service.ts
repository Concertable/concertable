import { Injectable } from '@angular/core';
import { ToastService } from '../toast.service';
import { ActiveToast, ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class EventToastService extends ToastService {
  constructor(private router: Router, toastr: ToastrService) {
    super(toastr)
  }

  showUpdated() {
    this.showSuccess("Successfully updated the event","Updated")
  }
  showAutoCreated() {
    this.showSuccess("Successfully created the event","Created")
  }
  
  showRecommended(eventId: number) {
    const toast: ActiveToast<any> = this.toastr.info(
      'A New Recommendation has just popped up that might interest you. Check it out!',
      'New Recommendation!',
      {
        closeButton: true,
        tapToDismiss: false,
        disableTimeOut: false,
      }
    );

    toast.onTap.subscribe(() => {
      this.router.navigate(['/find/event', eventId]);
      this.toastr.clear();
    });
  }
}
