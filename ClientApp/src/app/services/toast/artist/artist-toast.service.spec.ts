import { TestBed } from '@angular/core/testing';

import { ArtistToastService } from './artist-toast.service';

describe('ArtistToastService', () => {
  let service: ArtistToastService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ArtistToastService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
