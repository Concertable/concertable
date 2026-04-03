import { TestBed } from '@angular/core/testing';

import { ListingToastServiceService } from './listing-toast-service.service';

describe('ListingToastServiceService', () => {
  let service: ListingToastServiceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ListingToastServiceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
