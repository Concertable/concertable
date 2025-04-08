import { Component, OnInit } from '@angular/core';
import { LoginCredentials } from '../../models/login-credentials';
import { AuthService } from '../../services/auth/auth.service';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: false,
  
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent implements OnInit {
  title: string = 'Login Page'
  returnUrl?: string;
  credentials: LoginCredentials = {
    email: '',
    password: ''
  };
  
  constructor(
    private authService: AuthService, 
    private router: Router,
    private route: ActivatedRoute) { }

  ngOnInit(): void {
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'];
  }

  onLogin() {
    this.authService.login(this.credentials, this.returnUrl)
      .subscribe();
  }

  onForgotPassword(event: MouseEvent) {
    event.preventDefault();
    this.authService.forgotPassword(this.credentials.email).subscribe()
  }
}
