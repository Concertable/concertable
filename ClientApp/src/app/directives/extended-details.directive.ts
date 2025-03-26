import { Directive } from '@angular/core';
import { AuthService } from '../services/auth/auth.service';
import { BlobStorageService } from '../services/blob-storage/blob-storage.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Venue } from '../models/venue';
import { Artist } from '../models/artist';
import { Event } from '../models/event';
import { DetailsDirective } from './details/details.directive';
import { NavItem } from '../models/nav-item';

@Directive({
  selector: '[appExtendedDetails]',
  standalone: false
})
export abstract class ExtendedDetailsDirective<T extends Venue | Artist | Event> extends DetailsDirective<T> {
  navItems: NavItem[] = [
    { name: 'Info', fragment: 'info' },
    { name: 'Videos', fragment: 'videos' },
    { name: 'Reviews', fragment: 'reviews' }
  ];

  constructor(
    protected blobStorageService: BlobStorageService, 
    authService: AuthService,
    route: ActivatedRoute,
    router: Router
  ) {
    super(authService, route, router);
  }

  exists(section: string): boolean {
    return this.navItems.some(n => n.name === section);
  }
}
