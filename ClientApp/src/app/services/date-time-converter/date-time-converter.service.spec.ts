import { TestBed } from '@angular/core/testing';

import { DateTimeConverterService } from './date-time-converter.service';

describe('DateTimeConverterService', () => {
  let service: DateTimeConverterService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DateTimeConverterService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
