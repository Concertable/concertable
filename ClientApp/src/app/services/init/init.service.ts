import { Injectable } from '@angular/core';
import { AuthService } from '../auth/auth.service';
import { forkJoin } from 'rxjs';
import { Router } from '@angular/router';
import { SignalRService } from '../signalr/signalr.service';
import { EventService } from '../event/event.service';

@Injectable({
  providedIn: 'root'
})
export class InitService {

  constructor(
    private authService: AuthService, 
    private signalRService: SignalRService,
    private eventService: EventService
  ) { }

  init(): Promise<void> {
    return new Promise((resolve) => {
      this.authService.getCurrentUser().subscribe({
        next: (user) => {
          console.log(user);
          if (user) {
            this.signalRService.createHubConnections();
          }
          resolve(); // ✅ still resolves when successful
        },
        error: (err) => {
          console.warn('Init failed to get current user:', err);
          resolve(); // ✅ make sure app still starts
        }
      });
    });
  }  
}
