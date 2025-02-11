import { Directive, Input, OnInit } from '@angular/core';
import { CoreEntity } from '../../models/core-entity';
import { AuthService } from '../../services/auth/auth.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { NavItem } from '../../models/nav-item';

@Directive({
  selector: '[appCoreEntityDetails]',
  standalone: false,
})
export abstract class CoreEntityDetailsDirective<T extends CoreEntity> implements OnInit {
  @Input() entity?: T;
  @Input() editMode?: boolean;

  navItems: NavItem[] = [
    { name: 'Info', fragment: 'info' },
    { name: 'Events', fragment: 'events' },
    { name: 'Videos', fragment: 'videos' },
    { name: 'Reviews', fragment: 'reviews' }
  ];

  constructor(
    protected authService: AuthService,
    protected route: ActivatedRoute,
    protected router: Router
  ) { }

  ngOnInit() {
    if (!this.entity) {
      this.route.queryParams.subscribe(params => {
        const id = params['id'];
        if (id) {
          this.loadDetails(id).subscribe(entity => this.entity = entity);
        }
      });
    }
  }

  abstract loadDetails(id: number): Observable<T>;

  exists(section: string): boolean {
    return this.navItems.some(n => n.name === section);
  }
}
