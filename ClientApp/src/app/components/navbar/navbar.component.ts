import { AfterViewInit, Component, ContentChild, ElementRef, HostListener, TemplateRef, ViewChild } from '@angular/core';
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
  @ViewChild('menu', { static: false }) menu!: ElementRef;
  @ViewChild('btnMenu', { static: true }) btnMenu!: ElementRef;

  isMenuOpen: boolean = false;

  constructor(protected authService: AuthService, private router: Router) {}

  toggleMenu() {
    this.isMenuOpen = !this.isMenuOpen;
  }

  onLogoClick() {
    this.router.navigateByUrl('/');
  }

  @HostListener('document:click', ['$event'])
  handleClickOutside(event: MouseEvent) {
    if (!this.menu) return;
    
    const clickedInsideMenu = this.menu.nativeElement.contains(event.target);

    let clickedMenuButton = false;
    if (this.btnMenu && this.btnMenu.nativeElement) {
      clickedMenuButton = this.btnMenu.nativeElement.contains(event.target);
    }
    
    if (this.isMenuOpen && !clickedInsideMenu && !clickedMenuButton) {
      this.isMenuOpen = false;
    }
  }

}
