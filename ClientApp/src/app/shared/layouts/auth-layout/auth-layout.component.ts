import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-auth-layout',
  standalone: false,
  
  templateUrl: './auth-layout.component.html',
  styleUrl: './auth-layout.component.scss'
})
export class AuthLayoutComponent {
  @Input() title?: string;
  titleStyle: { [key: string]: string; } = { 'text-align': 'center' };
  @Input() imageSrc?: string = '';
}
