import { TestBed } from '@angular/core/testing';

import { StripeAccountToastService } from './stripe-account-toast.service';

describe('StripeAccountToastService', () => {
  let service: StripeAccountToastService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(StripeAccountToastService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
