import { Component, Input } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-config-item-not-found',
  standalone: false,
  
  templateUrl: './config-item-not-found.component.html',
  styleUrl: './config-item-not-found.component.scss'
})
export class ConfigItemNotFoundComponent {
  @Input() itemName?: string;

  constructor(private router: Router, private route: ActivatedRoute) { }

  createItem() {
    let parent = this.route;
    
    while (parent.parent && parent.parent !== this.route.root) 
      parent = parent.parent;
    
    this.router.navigate(['create'], { relativeTo: parent });
  }
}
