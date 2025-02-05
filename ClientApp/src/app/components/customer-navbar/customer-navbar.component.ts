import { Component } from '@angular/core';
import { AuthService } from '../../services/auth/auth.service';
import { Router } from '@angular/router';
import { Role } from '../../models/role';

@Component({
  selector: 'app-customer-navbar',
  standalone: false,
  templateUrl: './customer-navbar.component.html',
  styleUrl: './customer-navbar.component.scss'
})
export class CustomerNavbarComponent {
  constructor(public authService: AuthService, private router: Router) { }

  Role = Role

  logout() {
    console.log("called logout")
    this.authService.logout().subscribe();
    this.router.navigateByUrl('/');
  }
}
