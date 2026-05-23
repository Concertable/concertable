import { Component } from '@angular/core';
import { AuthService } from '../../services/auth/auth.service';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { ActivatedRoute, Router } from '@angular/router';


@Component({
  selector: 'app-profile',
  standalone: false,
  
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent {
  constructor(
    protected authService: AuthService,
    private router: Router
  ) { }

  onProfile() {
    this.router.navigateByUrl('/profile');
  }

  onEditProfile() {
    this.router.navigate(['/profile/my'], {
      queryParams: { editMode: true }
    });
  }  

  onViewProfile() {
    this.router.navigateByUrl('/profile/my');
  }

  onLogOut() : void {
    this.authService.logout().subscribe();
  }
}
