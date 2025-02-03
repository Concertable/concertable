import { Component } from '@angular/core';
import { LoginCredentials } from '../../models/login-credentials';
import { AuthService } from '../../services/auth/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: false,
  
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  
  credentials: LoginCredentials = {
    email: '',
    password: ''
    };
  
  constructor(private authService: AuthService, private router: Router) { }

  login() {
    this.authService.login(this.credentials)
      .subscribe({
        next: () => {
          console.log('Login successful');
          this.router.navigateByUrl('/');
          console.log("call2")
        },
        error: (error) => {
          console.error('Login failed', error);
          alert('Login failed. Please check your credentials and try again.');
        }
      });
  }
}
