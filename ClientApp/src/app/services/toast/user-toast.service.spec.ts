import { TestBed } from '@angular/core/testing';

import { UserToastService } from './user-toast.service';

describe('UserToastService', () => {
  let service: UserToastService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(UserToastService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
