import { TestBed } from '@angular/core/testing';

import { UrlSerializerService } from './url-serializer.service';

describe('UrlSerializerService', () => {
  let service: UrlSerializerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(UrlSerializerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
