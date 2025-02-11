import { Component, Input } from '@angular/core';
import { Header } from '../../models/header';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-header',
  standalone: false,
  
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent<T extends Header> {
  @Input() header?: T;

  constructor(private router: Router, private route: ActivatedRoute) { }

  onClick(): void {
    console.log("heee")
    if(this.header)
      this.router.navigate([this.header.type], { 
        relativeTo: this.route,
        queryParams: { id: this.header.id }  
    });
  }
}
