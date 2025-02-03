import { Component } from '@angular/core';
import { AuthService } from '../../services/auth/auth.service';
import { Router } from '@angular/router';
import { Role } from '../../models/role';

@Component({
  selector: 'app-customer-header',
  standalone: false,
  templateUrl: './customer-header.component.html',
  styleUrl: './customer-header.component.scss'
})
export class CustomerHeaderComponent {
  constructor(public authService: AuthService, private router: Router) { }

  Role = Role

  logout() {
    console.log("called logout")
    this.authService.logout().subscribe();
    this.router.navigateByUrl('/');
  }
}
