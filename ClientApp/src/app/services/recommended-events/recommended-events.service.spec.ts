import { TestBed } from '@angular/core/testing';

import { RecommendedEventsService } from './recommended-events.service';

describe('RecommendedEventsService', () => {
  let service: RecommendedEventsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(RecommendedEventsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
