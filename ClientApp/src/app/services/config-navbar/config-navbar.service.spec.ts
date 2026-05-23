import { TestBed } from '@angular/core/testing';

import { ConfigNavbarService } from './config-navbar.service';

describe('ConfigNavbarService', () => {
  let service: ConfigNavbarService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ConfigNavbarService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
