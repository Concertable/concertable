import { Directive } from '@angular/core';
import { AuthService } from '../../services/auth/auth.service';
import { BlobStorageService } from '../../services/blob-storage/blob-storage.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Venue } from '../../models/venue';
import { Artist } from '../../models/artist';
import { Event } from '../../models/event';
import { DetailsDirective } from '../details/details.directive';
import { NavItem } from '../../models/nav-item';
import { ToastService } from '../../services/toast/toast.service';
import { Genre } from '../../models/genre';
import { GenreService } from '../../services/genre/genre.service';
import { EventViewType } from '../../models/event-view-type';

@Directive({
  selector: '[appExtendedDetails]',
  standalone: false
})
export abstract class ExtendedDetailsDirective<T extends Venue | Artist | Event> extends DetailsDirective<T> {
  navItems: NavItem[] = [
    { name: 'About', fragment: 'about' },
    { name: 'Videos', fragment: 'videos' },
    { name: 'Reviews', fragment: 'reviews' }
  ];
  genres: Genre[] = [];

  EventViewType = EventViewType;

  constructor(
    protected blobStorageService: BlobStorageService, 
    private genreService: GenreService,
    authService: AuthService,
    route: ActivatedRoute,
    router: Router,
    toastService: ToastService
  ) {
    super(authService, route, router, toastService);
  }

  override ngOnInit(): void {
      super.ngOnInit();
      this.genreService.getAll().subscribe(g => this.genres = g)
  }

  exists(section: string): boolean {
    return this.navItems.some(n => n.name === section);
  }
}
