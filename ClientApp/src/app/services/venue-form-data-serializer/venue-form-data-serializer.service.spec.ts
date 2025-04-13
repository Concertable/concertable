import { TestBed } from '@angular/core/testing';

import { VenueFormDataSerializerService } from './venue-form-data-serializer.service';

describe('VenueFormDataSerializerService', () => {
  let service: VenueFormDataSerializerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(VenueFormDataSerializerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
