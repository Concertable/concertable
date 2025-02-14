import { TestBed } from '@angular/core/testing';

import { EventCheckoutService } from './event-checkout.service';

describe('EventCheckoutService', () => {
  let service: EventCheckoutService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(EventCheckoutService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
