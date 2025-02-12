import { TestBed } from '@angular/core/testing';

import { VenueToastService } from './venue-toast.service';

describe('VenueToastService', () => {
  let service: VenueToastService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(VenueToastService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
