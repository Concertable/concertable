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

  init() {
    this.authService.getCurrentUser().subscribe((user) =>
    {
      console.log(user);
      if(user) this.signalRService.createHubConnections();
    }
    );
  }
}
