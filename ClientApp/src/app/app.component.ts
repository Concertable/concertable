import { Component, OnInit } from '@angular/core';
import { InitService } from './services/init/init.service';
import { AuthService } from './services/auth/auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: false,
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'concertable-app';

  constructor() { }
}
