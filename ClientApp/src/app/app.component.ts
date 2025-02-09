import { Component, OnInit } from '@angular/core';
import { InitService } from './services/init/init.service';
import { AuthService } from './services/auth/auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: false,
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  title = 'concertable-app';

  constructor(private initService: InitService) { }

  ngOnInit(): void {
    this.initService.init();
    // console.log(google);
  }
}
