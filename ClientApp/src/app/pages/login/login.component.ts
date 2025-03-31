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

  login() {
    this.authService.login(this.credentials, this.returnUrl)
      .subscribe();
  }
}
