import { Component } from '@angular/core';
import { RegisterCredentials } from '../../models/register-credentials';
import { AuthService } from '../../services/auth/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  standalone: false,
  
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  credentials: RegisterCredentials = {
    email: '',
    password: '',
    role: 'Customer'
  };

  constructor(private authService: AuthService, private router: Router) { }
  
  register() {
    if(this.credentials)
      this.authService.register(this.credentials).subscribe();
  }

}
