import { Component, Input } from '@angular/core';
import { Header } from '../../models/header';

@Component({
  selector: 'app-header',
  standalone: false,
  
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent {
  @Input() header?: Header;
}
