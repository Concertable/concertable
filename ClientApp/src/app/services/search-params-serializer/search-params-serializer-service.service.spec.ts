import { TestBed } from '@angular/core/testing';

import { SearchParamsSerializerServiceService } from './search-params-serializer-service.service';

describe('SearchParamsSerializerServiceService', () => {
  let service: SearchParamsSerializerServiceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SearchParamsSerializerServiceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
