import { TestBed } from '@angular/core/testing';

import { PreferenceToastService } from './preference-toast.service';

describe('PreferenceToastService', () => {
  let service: PreferenceToastService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PreferenceToastService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
