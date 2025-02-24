import { Injectable } from '@angular/core';
import { ToastService } from '../toast/toast.service';

@Injectable({
  providedIn: 'root'
})
export class PopupService {

  constructor(private toastService: ToastService) { }

  openPopup(url: string, width: number = 600, height: number = 700): Window | null {
    const left = (window.innerWidth - width) / 2;
    const top = (window.innerHeight - height) / 2;

    const popup = window.open(
      url,
      'popupWindow',
      `width=${width},height=${height},top=${top},left=${left},resizable=yes,scrollbars=yes`
    );

    if (!popup) {
      this.toastService.showError("Popup blocked! Please allow popups for this site.", "Popup Blocked!");
    }

    return popup;
  }

  monitorPopup(popup: Window | null, callback: () => void): void {
    if (!popup) return;
  
    const interval = setInterval(() => {
      if (popup.closed) {
        clearInterval(interval);
        callback(); 
      }
    }, 1000);
  }  
}
