import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { first } from 'rxjs/operators';
import { AuthService } from './auth/auth.service';
import { User } from '../models/user';
import { Item } from '../models/item';
import { Role } from '../models/role';

interface RoleNavigationMapping {
  role: Role;
  isOwner: (resource: any, user: User) => boolean;
  routes: {
    [resourceType: string]: (resource: any) => string[];
  };
}

const roleMappings: RoleNavigationMapping[] = [
  {
    role: 'ArtistManager',
    isOwner: (resource, user) => resource?.artist?.email === user?.email,
    routes: {
      event: (event) => ['artist', 'my', 'events', 'event', event.id],
      artist: () => ['artist', 'my']
    }
  },
  {
    role: 'VenueManager',
    isOwner: (resource, user) => resource?.venue?.email === user?.email,
    routes: {
      event: (event) => ['venue', 'my', 'events', 'event', event.id],
      venue: () => ['venue', 'my']
    }
  }
];

const fallbackRoutes: { [resourceType: string]: (resource: any) => string[] } = {
  event: (e) => ['find', 'event', e.id],
  venue: (v) => ['find', 'venue', v.id],
  artist: (a) => ['find', 'artist', a.id]
};

@Injectable({
  providedIn: 'root'
})
export class RoleNavigationService {

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  navigateToResource(resource: any, resourceType: string) {
    this.authService.currentUser$.subscribe(user => {
      if (!user) {
        // No user? Use default fallback.
        this.router.navigate(fallbackRoutes[resourceType]?.(resource));
        return;
      }
  
      const mapping = roleMappings.find(m =>
        this.authService.isRole(m.role) && m.isOwner(resource, user)
      );
  
      const route = mapping?.routes[resourceType]
        ? mapping.routes[resourceType](resource)
        : fallbackRoutes[resourceType]?.(resource) ?? ['find', resourceType, resource.id];
  
      this.router.navigate(route);
    });
  }

  navigateToDefault(resource: any, resourceType: string) {
    this.router.navigate(fallbackRoutes[resourceType]?.(resource));
  }
}
