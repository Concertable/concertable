import { TestBed } from '@angular/core/testing';
import { CanActivateFn } from '@angular/router';

import { eventCheckoutGuard } from '../event-checkout.guard';

describe('eventCheckoutGuard', () => {
  const executeGuard: CanActivateFn = (...guardParameters) => 
      TestBed.runInInjectionContext(() => eventCheckoutGuard(...guardParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeGuard).toBeTruthy();
  });
});
