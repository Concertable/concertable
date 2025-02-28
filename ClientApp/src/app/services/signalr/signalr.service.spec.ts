import { TestBed } from '@angular/core/testing';

import { PaymentHubService } from './signalr.service';

describe('PaymentHubService', () => {
  let service: PaymentHubService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PaymentHubService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
