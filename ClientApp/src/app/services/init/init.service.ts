import { Injectable } from '@angular/core';
import { AuthService } from '../auth/auth.service';
import { forkJoin } from 'rxjs';
import { Router } from '@angular/router';
import { PaymentHubService } from '../payment-hub/payment-hub.service';

@Injectable({
  providedIn: 'root'
})
export class InitService {

  constructor(private authService: AuthService, private paymentHubService: PaymentHubService) { }

  init() {
    this.authService.getCurrentUser().subscribe((user) =>
    {
      console.log(user);
      if(user) this.paymentHubService.createHubConnection();
    }
    );
  }
}
