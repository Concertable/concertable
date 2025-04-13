import { Injectable } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class NavigationService {
  private previousUrl: string = '/';
  private currentUrl: string = '/';

  constructor(private router: Router) {
    this.router.events.subscribe(event => {
      if (event instanceof NavigationEnd) {
        this.previousUrl = this.currentUrl;
        this.currentUrl = event.urlAfterRedirects;
      }
    });
  }

  getPreviousUrl(): string {
    return this.previousUrl;
  }

  navigateToPreviousUrl(): void {
    this.router.navigateByUrl(this.previousUrl);
  }
}
