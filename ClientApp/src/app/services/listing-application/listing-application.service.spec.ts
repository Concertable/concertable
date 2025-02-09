import { TestBed } from '@angular/core/testing';

import { ListingApplicationService } from './listing-application.service';

describe('ListingApplicationService', () => {
  let service: ListingApplicationService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ListingApplicationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
