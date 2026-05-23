import { TestBed } from '@angular/core/testing';

import { FormDataSerializerService } from './form-data-serializer.service';

describe('FormDataSerializerService', () => {
  let service: FormDataSerializerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(FormDataSerializerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
