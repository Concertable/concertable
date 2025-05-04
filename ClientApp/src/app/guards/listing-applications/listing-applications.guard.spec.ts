import { TestBed } from '@angular/core/testing';
import { CanActivateFn } from '@angular/router';

import { listingApplicationsGuard } from './listing-applications.guard';

describe('listingApplicationsGuard', () => {
  const executeGuard: CanActivateFn = (...guardParameters) => 
      TestBed.runInInjectionContext(() => listingApplicationsGuard(...guardParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeGuard).toBeTruthy();
  });
});
