import { Injectable } from '@angular/core';
import { AuthService } from '../auth/auth.service';
import { forkJoin } from 'rxjs';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class InitService {

  constructor(private authService: AuthService, private router: Router) { }

  init() {
    this.authService.getCurrentUser().subscribe((user) =>
    {
      console.log("HI THIS IS A TEST")
      console.log(user);
      //this.authService.navigateByRole(user.role)
    }
    );
  }
}
