import { Component } from '@angular/core';
import { AuthService } from '../../services/auth/auth.service';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';


@Component({
  selector: 'app-profile',
  standalone: false,
  
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent {
  profileListOpen = false;

  constructor(protected authService: AuthService) { }

  onProfile() {
    this.profileListOpen = !this.profileListOpen;
  }

  onEditProfile() {

  }

  onLogOut() : void {
    this.authService.logout().subscribe();
  }
}
