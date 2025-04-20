import { Injectable } from '@angular/core';
import { AuthService } from '../auth/auth.service';
import { forkJoin } from 'rxjs';
import { Router } from '@angular/router';
import { SignalRService } from '../signalr/signalr.service';
import { EventService } from '../event/event.service';
import { HttpContext } from '@angular/common/http';
import { SKIP_ERROR_HANDLER } from '../../shared/http/http-context.token';

@Injectable({
  providedIn: 'root'
})
export class InitService {

  constructor(
    private authService: AuthService, 
    private signalRService: SignalRService,
    private eventService: EventService,
    private router: Router
  ) { }

  init(): Promise<void> {
    return new Promise((resolve) => {
      const context = new HttpContext().set(SKIP_ERROR_HANDLER, true);
      this.authService.getCurrentUser(context).subscribe({
        next: (user) => {
          console.log(user);
          if (user) {
            this.signalRService.createHubConnections();
            this.router.navigateByUrl(user.baseUrl)
          }
          resolve();
        },
        error: (err) => {
          console.warn('Init failed to get current user:', err);
          resolve(); // make sure app still starts
        }
      });
    });
  }  
}
