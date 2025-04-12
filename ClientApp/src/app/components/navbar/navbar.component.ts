import { AfterViewInit, Component, ContentChild, ElementRef, TemplateRef, ViewChild } from '@angular/core';
import { AuthService } from '../../services/auth/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-navbar',
  standalone: false,
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss'
})
export class NavbarComponent {
  @ContentChild('content') content!: TemplateRef<any>;
  isMenuOpen: boolean = false;

  constructor(protected authService: AuthService, private router: Router) {}

  toggleMenu() {
    this.isMenuOpen = !this.isMenuOpen;
  }

  onLogoClick() {
    this.router.navigateByUrl('/');
  }
}
