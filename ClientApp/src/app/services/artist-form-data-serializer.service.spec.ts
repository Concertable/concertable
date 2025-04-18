import { TestBed } from '@angular/core/testing';

import { ArtistFormDataSerializerService } from './artist-form-data-serializer.service';

describe('ArtistFormDataSerializerService', () => {
  let service: ArtistFormDataSerializerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ArtistFormDataSerializerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
