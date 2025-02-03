import { Component, Input } from '@angular/core';
import { NavItem } from '../../models/nav-item';
import { Router } from '@angular/router';

@Component({
  selector: 'app-scrollspy',
  standalone: false,
  
  templateUrl: './scrollspy.component.html',
  styleUrl: './scrollspy.component.scss'
})
export class ScrollspyComponent {
  @Input() navItems: NavItem[] = [];

}
