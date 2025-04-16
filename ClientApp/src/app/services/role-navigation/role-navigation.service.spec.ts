import { TestBed } from '@angular/core/testing';

import { RoleNavigationService } from './role-navigation.service';

describe('RoleNavigationService', () => {
  let service: RoleNavigationService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(RoleNavigationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
