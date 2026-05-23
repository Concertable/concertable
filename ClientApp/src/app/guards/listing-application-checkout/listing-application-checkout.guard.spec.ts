import { TestBed } from '@angular/core/testing';
import { CanActivateFn } from '@angular/router';

import { listingApplicationCheckoutGuard } from './listing-application-checkout.guard';

describe('listingApplicationCheckoutGuard', () => {
  const executeGuard: CanActivateFn = (...guardParameters) => 
      TestBed.runInInjectionContext(() => listingApplicationCheckoutGuard(...guardParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeGuard).toBeTruthy();
  });
});
