import { TestBed } from '@angular/core/testing';

import { StripeToastService } from './stripe-toast.service';

describe('StripeToastService', () => {
  let service: StripeToastService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(StripeToastService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
