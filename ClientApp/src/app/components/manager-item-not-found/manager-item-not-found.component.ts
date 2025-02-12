import { Component, Input } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-manager-item-not-found',
  standalone: false,
  
  templateUrl: './manager-item-not-found.component.html',
  styleUrl: './manager-item-not-found.component.scss'
})
export class ManagerItemNotFoundComponent {
  @Input() itemName?: string;

  constructor(private router: Router, private route: ActivatedRoute) { }

  createItem() {
      this.router.navigate(['../create'], { relativeTo: this.route });
  }
}
