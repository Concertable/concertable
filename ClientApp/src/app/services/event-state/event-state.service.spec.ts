import { TestBed } from '@angular/core/testing';

import { EventStateService } from './event-state.service';

describe('EventStateService', () => {
  let service: EventStateService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(EventStateService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
