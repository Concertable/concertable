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
      this.router.navigate(['create'], { relativeTo: this.route });
  }
}
