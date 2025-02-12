import { Directive, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AuthService } from '../../services/auth/auth.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { NavItem } from '../../models/nav-item';
import { Venue } from '../../models/venue';
import { Artist } from '../../models/artist';
import { Event } from '../../models/event';

@Directive({
  selector: '[appDetails]',
  standalone: false,
})
export abstract class DetailsDirective<T extends Venue | Artist | Event> implements OnInit {
  @Input() entity?: T;
  @Input() editMode?: boolean;
  @Output() entityChange = new EventEmitter<T>

  navItems: NavItem[] = [
    { name: 'Info', fragment: 'info' },
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

  onChangeDetected() {
    this.entityChange.emit(this.entity);
  }
}
