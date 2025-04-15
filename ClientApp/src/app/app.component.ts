import { Component, OnInit } from '@angular/core';
import { InitService } from './services/init/init.service';
import { AuthService } from './services/auth/auth.service';
import { NavigationError, Router } from '@angular/router';
import { ToastService } from './services/toast/toast.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: false,
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  title = 'concertable-app';
  constructor(private router: Router, private toastService: ToastService) {}

  ngOnInit(): void {
    this.router.events.subscribe(event => {
      if (event instanceof NavigationError) {
        this.toastService.showWarning('That page does not exist', '404 Not Found');
        this.router.navigate(['/']);
      }
    });
  }
}