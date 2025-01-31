import { Component, OnInit } from '@angular/core';
import { InitService } from './services/init/init.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: false,
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  title = 'ClientApp';

  constructor(private initService: InitService) { }

  ngOnInit(): void {
    this.initService.init();
    console.log(google);
  }
}
